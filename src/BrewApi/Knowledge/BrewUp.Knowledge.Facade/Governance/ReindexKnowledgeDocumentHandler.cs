using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.SharedKernel.Embeddings;

namespace BrewUp.Knowledge.Facade.Governance;

public sealed class ReindexKnowledgeDocumentHandler(
    IChunkingStrategy chunkingStrategy,
    IEmbeddingGenerator embeddingGenerator,
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeChunkRepository chunkRepository,
    IKnowledgeChunkWriter chunkWriter,
    IKnowledgeVectorStore vectorStore,
    IWikiRepository wikiRepository)
{
    public async Task<ReindexKnowledgeDocumentResult?> HandleAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(
            documentId,
            cancellationToken);

        if (document is null)
            return null;

        await wikiRepository.MarkEvidenceUnavailableAsync(documentId, cancellationToken);
        await vectorStore.DeleteByDocumentIdAsync(documentId, cancellationToken);
        await chunkRepository.DeleteByDocumentIdAsync(documentId, cancellationToken);

        var chunks = chunkingStrategy.Split(document);
        await chunkWriter.StoreAsync(chunks, cancellationToken);

        foreach (var chunk in chunks)
        {
            var embedding = await embeddingGenerator.GenerateAsync(
                chunk.KnowledgeContent,
                cancellationToken);
            await vectorStore.StoreAsync(chunk, embedding, cancellationToken);
        }

        var wikiStatus = await wikiRepository.EnqueueAsync(documentId, cancellationToken);
        return new ReindexKnowledgeDocumentResult(documentId, chunks.Count, wikiStatus);
    }
}
