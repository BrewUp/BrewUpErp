using BrewUp.Knowledge.Infrastructure.Ingestion;

namespace BrewUp.Knowledge.Facade.Governance;

public sealed class GetKnowledgeDocumentHandler(
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeChunkRepository chunkRepository)
{
    public async Task<GetKnowledgeDocumentResult?> HandleAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(
            documentId,
            cancellationToken);

        if (document is null)
            return null;

        var chunkCount = await chunkRepository.CountByDocumentIdAsync(
            documentId,
            cancellationToken);

        return new GetKnowledgeDocumentResult(
            document.Id,
            document.Title,
            document.Scope.Name,
            document.Source.Name,
            document.Tags,
            document.ImportedAt,
            document.DocumentsContent,
            chunkCount);
    }
}
