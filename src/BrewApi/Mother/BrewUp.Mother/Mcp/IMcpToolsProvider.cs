using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace BrewUp.Mother.Mcp;

public interface IMcpToolsProvider
{
    /// <summary>
    /// Returns the union of tools exposed by every configured MCP server,
    /// suitable for passing to an LLM via <c>ChatOptions.Tools</c>.
    /// </summary>
    Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the cached <see cref="McpClient"/> for a specific MCP server
    /// (case-insensitive: "masterdata", "warehouse", "sales") so callers can
    /// invoke tools deterministically, bypassing the LLM.
    /// </summary>
    Task<McpClient> GetClientAsync(string serverName, CancellationToken cancellationToken);

    /// <summary>
    /// Calls a tool on the named MCP server and deserializes the structured
    /// content to <typeparamref name="TResult"/>. Returns <c>default</c> when
    /// the server returns no structured content.
    /// </summary>
    Task<TResult?> CallToolAsync<TResult>(
        string serverName,
        string toolName,
        object? arguments,
        CancellationToken cancellationToken);
}