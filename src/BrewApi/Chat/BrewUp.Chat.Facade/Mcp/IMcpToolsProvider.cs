using Microsoft.Extensions.AI;

namespace BrewUp.Chat.Facade.Mcp;

public interface IMcpToolsProvider
{
    Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken);
}