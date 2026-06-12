using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.McpServer;

public interface IKnowledgeFacade
{
    Task<object> SearchKnowledgeBaseAsync(SearchKnowledgeBaseRequest request, CancellationToken cancellationToken);
}