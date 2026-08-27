using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.McpServer;

public interface IKnowledgeFacade
{
    Task<object> SearchKnowledgeBaseAsync(SearchKnowledgeBaseRequest request, CancellationToken cancellationToken);

    Task<object> QueryWikiAsync(QueryWikiRequest request, CancellationToken cancellationToken);

    Task<object?> GetWikiPageAsync(string key, CancellationToken cancellationToken);

    Task<object?> GetWikiPageEvidenceAsync(Guid pageId, CancellationToken cancellationToken);
}