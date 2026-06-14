using BrewUp.Knowledge.Infrastructure.Ingestion;

namespace BrewUp.Knowledge.Facade.Governance;

public sealed class DeleteKnowledgeDocumentHandler(
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeChunkRepository chunkRepository,
    IKnowledgeVectorStore vectorStore)
{
    public async Task<bool> HandleAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(
            documentId,
            cancellationToken);

        if (document is null)
            return false;

        await vectorStore.DeleteByDocumentIdAsync(documentId, cancellationToken);
        await chunkRepository.DeleteByDocumentIdAsync(documentId, cancellationToken);
        return await documentRepository.DeleteAsync(documentId, cancellationToken);
    }
}
