using System.Threading;
using System.Threading.Tasks;

namespace BrewUp.Knowledge.McpServer;

public interface IKnowledgeFacade
{
    Task<object> SearchKnowledgeBaseAsync(CancellationToken cancellationToken);
}