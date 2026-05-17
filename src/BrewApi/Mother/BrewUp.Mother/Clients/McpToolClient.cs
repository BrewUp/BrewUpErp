using System.Net.Http.Json;
using System.Text.Json;
using BrewUp.Mother.CustomTypes;

namespace BrewUp.Mother.Clients;

internal sealed class McpToolClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<McpToolClient> logger) 
    : IMcpToolClient
{
    private readonly Dictionary<string, string> _servers =
        configuration
            .GetSection("McpServers")
            .Get<Dictionary<string, string>>()
        ?? [];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    
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
    
    private static TResponse? ExtractToolResult<TResponse>(JsonElement result)
    {
        if (!result.TryGetProperty("content", out var content))
            return result.Deserialize<TResponse>(JsonOptions);

        var first = content.EnumerateArray().FirstOrDefault();

        if (first.ValueKind == JsonValueKind.Undefined)
            return default;

        if (!first.TryGetProperty("text", out var text)) 
            return first.Deserialize<TResponse>(JsonOptions);
        
        var json = text.GetString();

        return string.IsNullOrWhiteSpace(json) 
            ? default 
            : JsonSerializer.Deserialize<TResponse>(json, JsonOptions);

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
}