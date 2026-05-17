namespace BrewUp.Mother.Clients;

internal sealed class McpToolClient : IMcpToolClient
{
    public Task<TResponse?> CallToolAsync<TResponse>(string serverName, string toolName, object arguments,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}