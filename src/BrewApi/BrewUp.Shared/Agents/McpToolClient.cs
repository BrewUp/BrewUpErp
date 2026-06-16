using System.Net.Http.Json;
using System.Text.Json;
using BrewUp.Mother.McpClients;
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

        return result;
    }
}
