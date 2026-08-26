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
public sealed class McpToolsProvider : IMcpToolsProvider, IAsyncDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

    private readonly McpServerOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<McpToolsProvider> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private readonly Dictionary<string, McpClient> _clients = new(StringComparer.Ordinal);
    private readonly List<McpClient> _retiredClients = [];
    private IReadOnlyList<AITool> _tools = [];
    private DateTimeOffset _lastRefreshUtc = DateTimeOffset.MinValue;
    private bool _hasRefreshed;

    public McpToolsProvider(
        McpServerOptions options,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
        : this(options, httpClientFactory, loggerFactory, TimeProvider.System)
    {
    }

    internal McpToolsProvider(
        McpServerOptions options,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        _timeProvider = timeProvider;
        _logger = loggerFactory.CreateLogger<McpToolsProvider>();
    }

    public async Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken)
    {
        if (IsFresh())
            return _tools;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsFresh())
                return _tools;

            var endpoints = new (string Name, string Url)[]
            {
                ("MasterData", _options.MasterDataUrl),
                ("Sales", _options.SalesUrl),
                ("Warehouse", _options.WarehouseUrl),
                ("Knowledge", _options.KnowledgeUrl)
            };

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
                    var client = await GetOrCreateClientAsync(name, url, cancellationToken).ConfigureAwait(false);
                    var tools = await client.ListToolsAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    newTools.AddRange(tools);

                    _logger.LogInformation("MCP {Name} ready with {Count} tools.", name, tools.Count);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to discover MCP tools from {Name} at {Url}. Tools from this server will be unavailable until the next refresh.",
                        name, url);

                    RetireClient(name);
                }
            }

            _tools = newTools;
            _lastRefreshUtc = _timeProvider.GetUtcNow();
            _hasRefreshed = true;

            return _tools;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsFresh()
        => _hasRefreshed && _timeProvider.GetUtcNow() - _lastRefreshUtc < RefreshInterval;

    private async Task<McpClient> GetOrCreateClientAsync(
        string name,
        string url,
        CancellationToken cancellationToken)
    {
        if (_clients.TryGetValue(name, out var client) && !client.Completion.IsCompleted)
            return client;

        if (client is not null)
            RetireClient(name, client);

        client = await McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(url),
                    Name = name
                },
                _httpClientFactory.CreateClient("mcp"),
                _loggerFactory),
            new McpClientOptions
            {
                // ProtocolVersion intentionally remains unset so SDK 2.x discovers modern
                // peers first and automatically negotiates down with legacy peers.
                InitializationTimeout = TimeSpan.FromSeconds(60),
                ClientInfo = new Implementation { Name = $"BrewUp.Mother.{name}", Version = "1.0.0" }
            },
            loggerFactory: _loggerFactory,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        _clients.Add(name, client);
        return client;
    }

    private void RetireClient(string name)
    {
        if (!_clients.Remove(name, out var client))
            return;

        _retiredClients.Add(client);
    }

    private void RetireClient(string name, McpClient client)
    {
        if (_clients.TryGetValue(name, out var activeClient)
            && ReferenceEquals(activeClient, client))
            _clients.Remove(name);

        _retiredClients.Add(client);
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            foreach (var (name, client) in _clients)
            {
                try
                {
                    await client.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing MCP client {Name}.", name);
                }
            }

            foreach (var client in _retiredClients.Distinct())
            {
                try
                {
                    await client.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing retired MCP client.");
                }
            }

            _clients.Clear();
            _retiredClients.Clear();
        }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }
}
