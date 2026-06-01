using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BrewUp.Mother.Mcp;

/// <summary>
/// Provides cached MCP tools shared across all chat requests.
/// Keeps the underlying <see cref="McpClient"/> instances alive for the
/// lifetime of the application and refreshes the tool catalog periodically
/// to recover from transient MCP server failures.
/// </summary>
public sealed class McpToolsProvider(
    McpServerOptions options,
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory)
    : IMcpToolsProvider, IAsyncDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger<McpToolsProvider> _logger = loggerFactory.CreateLogger<McpToolsProvider>();
    private readonly SemaphoreSlim _gate = new(1, 1);

    private Dictionary<string, McpClient> _clientsByName =
        new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<AITool> _tools = [];
    private DateTimeOffset _lastRefreshUtc = DateTimeOffset.MinValue;

    public async Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken)
    {
        await EnsureFreshAsync(cancellationToken).ConfigureAwait(false);
        return _tools;
    }

    public async Task<McpClient> GetClientAsync(string serverName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(serverName))
            throw new ArgumentException("Server name is required.", nameof(serverName));

        await EnsureFreshAsync(cancellationToken).ConfigureAwait(false);

        if (!_clientsByName.TryGetValue(serverName, out var client))
            throw new InvalidOperationException(
                $"No MCP client is available for server '{serverName}'. " +
                "Check configuration and that the remote MCP server is reachable.");

        return client;
    }

    public async Task<TResult?> CallToolAsync<TResult>(
        string serverName,
        string toolName,
        object? arguments,
        CancellationToken cancellationToken)
    {
        var client = await GetClientAsync(serverName, cancellationToken).ConfigureAwait(false);

        var args = ToArgumentDictionary(arguments);

        var result = await client
            .CallToolAsync(toolName, args, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (result.IsError == true)
        {
            var message = string.Join(
                " | ",
                result.Content.OfType<TextContentBlock>().Select(t => t.Text));
            throw new InvalidOperationException(
                $"MCP tool '{serverName}/{toolName}' returned an error: {message}");
        }

        if (result.StructuredContent is { } structured && structured.ValueKind != JsonValueKind.Null)
            return structured.Deserialize<TResult>(SerializerOptions);

        // Fallback: try the first text block as JSON
        var text = result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text;
        if (!string.IsNullOrWhiteSpace(text))
            return JsonSerializer.Deserialize<TResult>(text, SerializerOptions);

        return default;
    }

    private static Dictionary<string, object?>? ToArgumentDictionary(object? arguments)
    {
        switch (arguments)
        {
            case null:
                return null;
            case IReadOnlyDictionary<string, object?> ro:
                return new Dictionary<string, object?>(ro);
        }

        // Convert anonymous / POCO objects via JSON round-trip.
        var json = JsonSerializer.SerializeToElement(arguments, SerializerOptions);
        var dictionary = new Dictionary<string, object?>();
        foreach (var property in json.EnumerateObject())
            dictionary[property.Name] = JsonElementToObject(property.Value);
        
        return dictionary;
    }

    private static object? JsonElementToObject(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => element // pass object/array through as JsonElement
    };

    private async Task EnsureFreshAsync(CancellationToken cancellationToken)
    {
        if (IsFresh())
            return;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsFresh())
                return;

            await DisposeClientsAsync().ConfigureAwait(false);

            var endpoints = new (string Name, string Url)[]
            {
                ("MasterData", options.MasterDataUrl),
                ("Sales", options.SalesUrl),
                ("Warehouse", options.WarehouseUrl)
            };

            var newClients = new Dictionary<string, McpClient>(StringComparer.OrdinalIgnoreCase);
            var newTools = new List<AITool>();

            foreach (var (name, url) in endpoints)
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    _logger.LogWarning("MCP endpoint {Name} has no URL configured; skipping.", name);
                    continue;
                }

                try
                {
                    var client = await CreateClientAsync(name, url, cancellationToken).ConfigureAwait(false);
                    var tools = await client.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                    newClients[name] = client;
                    newTools.AddRange(tools);

                    _logger.LogInformation("MCP {Name} ready with {Count} tools.", name, tools.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to initialize MCP client {Name} at {Url}. Tools from this server will be unavailable until the next refresh.",
                        name, url);
                }
            }

            _clientsByName = newClients;
            _tools = newTools;
            _lastRefreshUtc = DateTimeOffset.UtcNow;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsFresh()
        => _tools.Count > 0 && DateTimeOffset.UtcNow - _lastRefreshUtc < RefreshInterval;

    private Task<McpClient> CreateClientAsync(string name, string url, CancellationToken cancellationToken)
        => McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(url),
                    Name = name
                },
                httpClientFactory.CreateClient("mcp"),
                loggerFactory),
            new McpClientOptions
            {
                InitializationTimeout = TimeSpan.FromSeconds(60),
                ClientInfo = new Implementation { Name = name, Version = "1.0.0" },
                Handlers = new McpClientHandlers
                {
                    NotificationHandlers =
                    [
                        new KeyValuePair<string, Func<JsonRpcNotification, CancellationToken, ValueTask>>(
                            "notifications/tools/list_changed",
                            async (_, ct) =>
                            {
                                ct.ThrowIfCancellationRequested();
                                await Task.CompletedTask;
                            })
                    ]
                }
            },
            loggerFactory: loggerFactory,
            cancellationToken: cancellationToken);

    private async Task DisposeClientsAsync()
    {
        foreach (var client in _clientsByName.Values)
        {
            try { await client.DisposeAsync().ConfigureAwait(false); }
            catch (Exception ex) { _logger.LogDebug(ex, "Error disposing MCP client."); }
        }
        _clientsByName.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeClientsAsync().ConfigureAwait(false);
        _gate.Dispose();
    }
}
