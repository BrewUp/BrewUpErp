using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Wiki;

namespace BrewUp.Knowledge.Facade.Governance;

public sealed class DeleteKnowledgeDocumentHandler(
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeChunkRepository chunkRepository,
    IKnowledgeVectorStore vectorStore,
    IWikiRepository wikiRepository)
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

        await wikiRepository.MarkEvidenceUnavailableAsync(documentId, cancellationToken);
        await vectorStore.DeleteByDocumentIdAsync(documentId, cancellationToken);
        await chunkRepository.DeleteByDocumentIdAsync(documentId, cancellationToken);
        return await documentRepository.DeleteAsync(documentId, cancellationToken);
    }
}
