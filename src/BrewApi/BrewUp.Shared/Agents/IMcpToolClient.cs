namespace BrewUp.Shared.Agents;

public interface IMcpToolClient
{
    Task<IReadOnlyCollection<McpToolMetadata>> ListToolsAsync(
        string serverName,
        CancellationToken cancellationToken);

    Task<TResponse?> CallToolAsync<TResponse>(
        string serverName,
        string toolName,
        object arguments,
        CancellationToken cancellationToken);
}
