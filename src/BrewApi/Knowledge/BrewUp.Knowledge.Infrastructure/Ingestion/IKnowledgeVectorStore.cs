using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.Infrastructure.Ingestion;

public interface IKnowledgeVectorStore
{
    Task StoreAsync(
        KnowledgeChunk chunk,
        EmbeddingVector embedding,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<KnowledgeVectorSearchResult>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int topK,
        CancellationToken cancellationToken);
}
