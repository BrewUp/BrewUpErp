using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.McpServer;

internal sealed class KnowledgeFacade(SearchKnowledgeHandler searchKnowledgeHandler) : IKnowledgeFacade
{
    public async Task<object> SearchKnowledgeBaseAsync(SearchKnowledgeBaseRequest request, CancellationToken cancellationToken)
    {
        return await searchKnowledgeHandler.HandleAsync(
            new SearchKnowledgeQuery(
                Query: request.Query,
                Scope: request.Scope,
                TopK: request.TopK),
            cancellationToken);
    }
}