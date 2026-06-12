using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BrewUp.Mother.Facade.Mcp;

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

    private readonly ILogger<McpToolsProvider> _logger = loggerFactory.CreateLogger<McpToolsProvider>();
    private readonly SemaphoreSlim _gate = new(1, 1);

    private List<McpClient> _clients = [];
    private IReadOnlyList<AITool> _tools = [];
    private DateTimeOffset _lastRefreshUtc = DateTimeOffset.MinValue;

    public async Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken)
    {
        if (IsFresh())
            return _tools;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsFresh())
                return _tools;

            await DisposeClientsAsync().ConfigureAwait(false);

            var endpoints = new (string Name, string Url)[]
            {
                ("MasterData", options.MasterDataUrl),
                ("Sales", options.SalesUrl),
                ("Warehouse", options.WarehouseUrl),
                ("Knowledge", options.KnowledgeUrl)
            };

            var newClients = new List<McpClient>(endpoints.Length);
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
                    var client = await CreateClientAsync(name, url, cancellationToken)
                        .ConfigureAwait(false);
                    var tools = await client.ListToolsAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    newClients.Add(client);
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

            _clients = newClients;
            _tools = newTools;
            _lastRefreshUtc = DateTimeOffset.UtcNow;

            return _tools;
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
                // Tune MCP client options here if needed, e.g. to adjust retry policies or timeouts.
                InitializationTimeout = TimeSpan.FromSeconds(60),
                ClientInfo = new Implementation { Name = name, Version = "1.0.0" },
                // Attach the handler
                Handlers = new McpClientHandlers
                {
                    NotificationHandlers =
                    [
                        new KeyValuePair<string, Func<JsonRpcNotification, CancellationToken, ValueTask>>(
                            "notifications/tools/list_changed",
                            async (_, ct) =>
                            {
                                ct.ThrowIfCancellationRequested();
                                
                                // Handle the notification here
                                await Task.CompletedTask;
                            })
                    ]
                }
            },
            loggerFactory: loggerFactory,
            cancellationToken: cancellationToken);

    private async Task DisposeClientsAsync()
    {
        foreach (var client in _clients)
        {
            try { await client.DisposeAsync().ConfigureAwait(false); }
            catch (Exception ex) { _logger.LogDebug(ex, "Error disposing MCP client."); }
        }
        _clients.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeClientsAsync()
            .ConfigureAwait(false);
        _gate.Dispose();
    }
}

