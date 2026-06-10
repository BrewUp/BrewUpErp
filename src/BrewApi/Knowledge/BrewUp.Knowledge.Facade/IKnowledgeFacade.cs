using BrewUp.Knowledge.Core.Search;
using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.Facade.Search;

namespace BrewUp.Knowledge.Facade;

public interface IKnowledgeFacade
{
    Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeDocumentRequest request,
        CancellationToken cancellationToken);

    Task<SearchKnowledgeBaseResult> SearchAsync(
        SearchKnowledgeBaseRequest request,
        CancellationToken cancellationToken);
}

internal sealed class KnowledgeFacade(
    IKnowledgeIngestionService ingestionService,
    IKnowledgeSearchEngine searchEngine) : IKnowledgeFacade
{
    public Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeDocumentRequest request,
        CancellationToken cancellationToken)
        => ingestionService.IngestAsync(request, cancellationToken);

    public async Task<SearchKnowledgeBaseResult> SearchAsync(
        SearchKnowledgeBaseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await searchEngine.SearchAsync(request.ToCoreRequest(), cancellationToken);
        return SearchKnowledgeBaseResult.From(result);
    }
}
