using BrewUp.Knowledge.Infrastructure.Ingestion;

namespace BrewUp.Knowledge.Facade.Governance;

public sealed class GetKnowledgeDocumentsHandler(
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeChunkRepository chunkRepository)
{
    public async Task<GetKnowledgeDocumentsResult> HandleAsync(
        CancellationToken cancellationToken)
    {
        var documents = await documentRepository.GetAllAsync(cancellationToken);
        var summaries = new List<KnowledgeDocumentSummary>(documents.Count);

        foreach (var document in documents)
        {
            var chunkCount = await chunkRepository.CountByDocumentIdAsync(
                document.Id,
                cancellationToken);

            summaries.Add(new KnowledgeDocumentSummary(
                document.Id,
                document.Title,
                document.Scope.Name,
                document.Source.Name,
                document.Tags,
                document.ImportedAt,
                chunkCount));
        }

        return new GetKnowledgeDocumentsResult(summaries);
    }
}
