using System.Net.Http.Json;
using System.Text.Json;
using BrewUp.Shared.CustomTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrewUp.Shared.Agents;

internal sealed class McpToolClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<McpToolClient> logger) 
    : IMcpToolClient
{
    private readonly Dictionary<string, string> _servers = LoadServers(configuration);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyCollection<McpToolMetadata>> ListToolsAsync(
        string serverName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_servers.TryGetValue(serverName, out var serverUrl))
            throw new InvalidOperationException($"MCP server '{serverName}' is not configured.");

        var client = httpClientFactory.CreateClient("mcp");

        var request = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString("N"),
            method = "tools/list",
            @params = new { }
        };

        logger.LogInformation(
            "Listing MCP tools from server {ServerName}",
            serverName);

        using var response = await client.PostAsJsonAsync(
            serverUrl,
            request,
            JsonOptions,
            cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "MCP tools/list failed. Server: {ServerName}, Status: {StatusCode}, Body: {Body}",
                serverName,
                response.StatusCode,
                raw);

            throw new InvalidOperationException(
                $"MCP tools/list failed for server '{serverName}' with status {(int)response.StatusCode}.");
        }

        var envelope = JsonSerializer.Deserialize<McpResponseEnvelope>(
            ExtractJsonPayload(raw),
            JsonOptions);

        if (envelope?.Error is not null)
        {
            logger.LogError(
                "MCP tools/list returned error. Server: {ServerName}, Error: {Error}",
                serverName,
                envelope.Error.Message);

            throw new InvalidOperationException(
                $"MCP tools/list returned an error for server '{serverName}': {envelope.Error.Message}");
        }

        if (envelope?.Result is null)
            return [];

        var tools = ExtractToolMetadata(envelope.Result.Value);

        logger.LogInformation(
            "MCP server {ServerName} exposed tools: {Tools}",
            serverName,
            string.Join(", ", tools.Select(tool => tool.Name)));

        return tools;
    }
    
    public async Task<TResponse?> CallToolAsync<TResponse>(string serverName, string toolName, object arguments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (!_servers.TryGetValue(serverName, out var serverUrl))
            throw new InvalidOperationException($"MCP server '{serverName}' is not configured.");

        var client = httpClientFactory.CreateClient("mcp");

        var request = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString("N"),
            method = "tools/call",
            @params = new
            {
                name = toolName,
                arguments
            }
        };

        logger.LogInformation(
            "Calling MCP tool {ServerName}.{ToolName}",
            serverName,
            toolName);

        using var response = await client.PostAsJsonAsync(
            serverUrl,
            request,
            JsonOptions,
            cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "MCP call failed. Server: {ServerName}, Tool: {ToolName}, Status: {StatusCode}, Body: {Body}",
                serverName,
                toolName,
                response.StatusCode,
                raw);

            return default;
        }

        var envelope = JsonSerializer.Deserialize<McpResponseEnvelope>(
            ExtractJsonPayload(raw),
            JsonOptions);

        if (envelope?.Error is null)
            return envelope?.Result is null
                ? default
                : ExtractToolResult<TResponse>(envelope.Result.Value);
        
        logger.LogError(
            "MCP returned error. Server: {ServerName}, Tool: {ToolName}, Error: {Error}",
            serverName,
            toolName,
            envelope.Error.Message);

        return default;
    }
    
    private static TResponse ExtractToolResult<TResponse>(JsonElement result)
    {
        if (result.TryGetProperty("structuredContent", out var structuredContent))
        {
            return structuredContent.Deserialize<TResponse>(JsonOptions)!;
        }

        if (result.TryGetProperty("content", out var content) &&
            content.ValueKind == JsonValueKind.Array &&
            content.GetArrayLength() > 0)
        {
            var first = content[0];

            if (first.TryGetProperty("text", out var textElement))
            {
                var text = textElement.GetString();

                if (typeof(TResponse) == typeof(string))
                    return (TResponse)(object)(text ?? string.Empty);

                if (!string.IsNullOrWhiteSpace(text) &&
                    LooksLikeJson(text))
                {
                    return JsonSerializer.Deserialize<TResponse>(
                        text,
                        JsonOptions)!;
                }

                throw new InvalidOperationException(
                    $"MCP tool returned plain text, but {typeof(TResponse).Name} was expected. Text starts with: {text?[..Math.Min(text.Length, 80)]}");
            }
        }

        return result.Deserialize<TResponse>(JsonOptions)!;
    }

    private static IReadOnlyCollection<McpToolMetadata> ExtractToolMetadata(JsonElement result)
    {
        if (!result.TryGetProperty("tools", out var toolsElement)
            || toolsElement.ValueKind != JsonValueKind.Array)
            return [];

        return toolsElement
            .EnumerateArray()
            .Select(tool =>
            {
                var name = tool.TryGetProperty("name", out var nameElement)
                    ? nameElement.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(name))
                    return null;

                var description = tool.TryGetProperty("description", out var descriptionElement)
                    ? descriptionElement.GetString()
                    : null;

                JsonElement? inputSchema = tool.TryGetProperty("inputSchema", out var inputSchemaElement)
                    ? inputSchemaElement.Clone()
                    : null;

                return new McpToolMetadata(name, description, inputSchema);
            })
            .OfType<McpToolMetadata>()
            .ToArray();
    }
    
    private static bool LooksLikeJson(string value)
    {
        var trimmed = value.TrimStart();
        return trimmed.StartsWith("{") || trimmed.StartsWith("[") || trimmed.StartsWith("\"");
    }

    private static string ExtractJsonPayload(string raw)
    {
        // MCP HTTP often responds as server-sent events:
        // event: message
        // data: { ...json... }
        if (!raw.StartsWith("event:", StringComparison.OrdinalIgnoreCase))
            return raw;

        var dataLine = raw
            .Split('\n')
            .FirstOrDefault(line => line.StartsWith("data:", StringComparison.OrdinalIgnoreCase));

        return dataLine is null 
            ? raw 
            : dataLine["data:".Length..].Trim();
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
