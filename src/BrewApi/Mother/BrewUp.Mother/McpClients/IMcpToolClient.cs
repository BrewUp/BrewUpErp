namespace BrewUp.Mother.McpClients;

public interface IMcpToolClient
{
    Task<TResponse?> CallToolAsync<TResponse>(
        string serverName,
        string toolName,
        object arguments,
        CancellationToken cancellationToken);
}