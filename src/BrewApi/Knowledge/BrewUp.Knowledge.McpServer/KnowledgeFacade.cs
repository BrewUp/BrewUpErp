using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.McpServer;

internal sealed class KnowledgeFacade(
    SearchKnowledgeHandler searchKnowledgeHandler,
    QueryWikiHandler queryWikiHandler,
    GetWikiPageHandler getWikiPageHandler,
    GetWikiPageEvidenceHandler getWikiPageEvidenceHandler) : IKnowledgeFacade
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

    public async Task<object> QueryWikiAsync(
        QueryWikiRequest request,
        CancellationToken cancellationToken)
        => await queryWikiHandler.HandleAsync(
            new QueryWiki(request.Query, request.Scope, request.TopK),
            cancellationToken);

    public async Task<object?> GetWikiPageAsync(
        string key,
        CancellationToken cancellationToken)
        => await getWikiPageHandler.HandleAsync(key, cancellationToken);

    public async Task<object?> GetWikiPageEvidenceAsync(
        Guid pageId,
        CancellationToken cancellationToken)
        => await getWikiPageEvidenceHandler.HandleAsync(pageId, cancellationToken);
}