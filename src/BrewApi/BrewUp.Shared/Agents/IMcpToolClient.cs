namespace BrewUp.Shared.Agents;

public interface IMcpToolClient
{
    Task<TResponse?> CallToolAsync<TResponse>(
        string serverName,
        string toolName,
        object arguments,
        CancellationToken cancellationToken);
}
