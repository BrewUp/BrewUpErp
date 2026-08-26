using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BrewUp.Shared.Agents;

internal sealed class McpToolClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILoggerFactory loggerFactory)
    : IMcpToolClient, IAsyncDisposable
{
    private readonly Dictionary<string, string> _servers = LoadServers(configuration);
    private readonly Dictionary<string, McpClient> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SemaphoreSlim> _clientLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<McpClient> _retiredClients = [];
    private readonly Lock _stateLock = new();
    private readonly ILogger<McpToolClient> _logger = loggerFactory.CreateLogger<McpToolClient>();

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyCollection<McpToolMetadata>> ListToolsAsync(
        string serverName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var client = await GetClientAsync(serverName, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Listing MCP tools from server {ServerName}",
            serverName);

        IList<McpClientTool> tools;
        try
        {
            tools = await client.ListToolsAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            RetireClient(serverName, client);
            throw;
        }

        var metadata = tools
            .Select(tool => new McpToolMetadata(
                tool.Name,
                tool.Description,
                tool.JsonSchema.Deserialize<JsonElement>(JsonOptions)))
            .ToArray();

        _logger.LogInformation(
            "MCP server {ServerName} exposed tools: {Tools}",
            serverName,
            string.Join(", ", metadata.Select(tool => tool.Name)));

        return metadata;
    }
    
    public async Task<TResponse?> CallToolAsync<TResponse>(string serverName, string toolName, object arguments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var client = await GetClientAsync(serverName, cancellationToken).ConfigureAwait(false);
        var toolArguments = ToArguments(arguments);

        _logger.LogInformation(
            "Calling MCP tool {ServerName}.{ToolName}",
            serverName,
            toolName);

        CallToolResult result;
        try
        {
            result = await client.CallToolAsync(
                    toolName,
                    toolArguments,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            RetireClient(serverName, client);
            throw;
        }

        if (result.IsError is true)
            throw new McpException(
                $"MCP tool '{serverName}.{toolName}' returned an error: {GetTextContent(result)}");

        return ExtractToolResult<TResponse>(result);
    }

    private async Task<McpClient> GetClientAsync(string serverName, CancellationToken cancellationToken)
    {
        if (!_servers.TryGetValue(serverName, out var serverUrl))
            throw new InvalidOperationException($"MCP server '{serverName}' is not configured.");

        var clientLock = GetClientLock(serverName);
        await clientLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (TryGetActiveClient(serverName, out var existingClient))
                return existingClient;

            var client = await McpClient.CreateAsync(
                    new HttpClientTransport(
                        new HttpClientTransportOptions
                        {
                            Endpoint = new Uri(serverUrl),
                            Name = serverName
                        },
                        httpClientFactory.CreateClient("mcp"),
                        loggerFactory),
                    new McpClientOptions
                    {
                        ClientInfo = new Implementation
                        {
                            Name = $"BrewUp.{serverName}",
                            Version = "1.0.0"
                        },
                        InitializationTimeout = TimeSpan.FromSeconds(60)
                    },
                    loggerFactory,
                    cancellationToken)
                .ConfigureAwait(false);

            lock (_stateLock)
                _clients[serverName] = client;

            return client;
        }
        finally
        {
            clientLock.Release();
        }
    }

    private SemaphoreSlim GetClientLock(string serverName)
    {
        lock (_stateLock)
        {
            if (_clientLocks.TryGetValue(serverName, out var clientLock))
                return clientLock;

            clientLock = new SemaphoreSlim(1, 1);
            _clientLocks.Add(serverName, clientLock);
            return clientLock;
        }
    }

    private bool TryGetActiveClient(string serverName, out McpClient client)
    {
        lock (_stateLock)
        {
            if (_clients.TryGetValue(serverName, out client!) && !client.Completion.IsCompleted)
                return true;

            if (client is not null)
            {
                _clients.Remove(serverName);
                _retiredClients.Add(client);
            }

            client = null!;
            return false;
        }
    }

    private void RetireClient(string serverName, McpClient client)
    {
        lock (_stateLock)
        {
            if (!_clients.TryGetValue(serverName, out var activeClient)
                || !ReferenceEquals(activeClient, client))
                return;

            _clients.Remove(serverName);
            _retiredClients.Add(client);
        }
    }

    private static IReadOnlyDictionary<string, object?> ToArguments(object arguments)
    {
        var element = JsonSerializer.SerializeToElement(arguments, JsonOptions);
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("MCP tool arguments must serialize to a JSON object.", nameof(arguments));

        return element.EnumerateObject()
            .ToDictionary(
                property => property.Name,
                property => (object?)property.Value.Clone(),
                StringComparer.Ordinal);
    }

    private static TResponse? ExtractToolResult<TResponse>(CallToolResult result)
    {
        if (result.StructuredContent is { } structuredContent)
            return structuredContent.Deserialize<TResponse>(JsonOptions);

        var text = GetTextContent(result);
        if (typeof(TResponse) == typeof(string))
            return (TResponse)(object)text;

        if (!string.IsNullOrWhiteSpace(text) && LooksLikeJson(text))
            return JsonSerializer.Deserialize<TResponse>(text, JsonOptions);

        if (string.IsNullOrWhiteSpace(text))
            return default;

        throw new InvalidOperationException(
            $"MCP tool returned plain text, but {typeof(TResponse).Name} was expected. Text starts with: {text[..Math.Min(text.Length, 80)]}");
    }

    private static string GetTextContent(CallToolResult result)
        => string.Join(
            Environment.NewLine,
            result.Content.OfType<TextContentBlock>().Select(content => content.Text));

    private static bool LooksLikeJson(string value)
    {
        var trimmed = value.TrimStart();
        return trimmed.StartsWith("{") || trimmed.StartsWith("[") || trimmed.StartsWith("\"");
    }

    public async ValueTask DisposeAsync()
    {
        McpClient[] clients;
        SemaphoreSlim[] clientLocks;
        lock (_stateLock)
        {
            clients = _clients.Values.Concat(_retiredClients).Distinct().ToArray();
            clientLocks = _clientLocks.Values.ToArray();
            _clients.Clear();
            _retiredClients.Clear();
            _clientLocks.Clear();
        }

        foreach (var client in clients)
            await client.DisposeAsync().ConfigureAwait(false);

        foreach (var clientLock in clientLocks)
            clientLock.Dispose();
    }

    private static Dictionary<string, string> LoadServers(IConfiguration configuration)
    {
        var servers = configuration
            .GetSection("McpServers")
            .Get<Dictionary<string, string>>()
            ?? [];

        var brewUpServers = configuration
            .GetSection("BrewUp:McpServers")
            .Get<Dictionary<string, string>>()
            ?? [];

        var knowledgeAgentMcp = configuration
            .GetSection("KnowledgeAgent:Mcp");

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in servers)
            result[key] = value;

        foreach (var (key, value) in brewUpServers)
        {
            if (key.EndsWith("Url", StringComparison.OrdinalIgnoreCase))
                result[key[..^3]] = value;
            else
                result[key] = value;
        }

        var knowledgeAgentServerName = knowledgeAgentMcp["ServerName"];
        var knowledgeAgentEndpoint = knowledgeAgentMcp["Endpoint"];

        if (!string.IsNullOrWhiteSpace(knowledgeAgentServerName)
            && !string.IsNullOrWhiteSpace(knowledgeAgentEndpoint))
            result[knowledgeAgentServerName] = knowledgeAgentEndpoint;

        return result;
    }
}
