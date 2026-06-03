using Microsoft.Extensions.AI;

namespace BrewUp.Mother.Facade.Mcp;

public interface IMcpToolsProvider
{
    Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken);
}