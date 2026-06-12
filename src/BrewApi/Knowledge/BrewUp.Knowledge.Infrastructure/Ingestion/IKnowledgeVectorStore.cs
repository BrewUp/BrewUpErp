using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Embeddings;

namespace BrewUp.Knowledge.Infrastructure.Ingestion;

public interface IKnowledgeVectorStore
{
    Task StoreAsync(
        KnowledgeChunk chunk,
        EmbeddingVector embedding,
        CancellationToken cancellationToken);
}
