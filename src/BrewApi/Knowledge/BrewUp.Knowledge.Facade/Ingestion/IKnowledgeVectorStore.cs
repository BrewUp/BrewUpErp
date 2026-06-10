using BrewUp.Knowledge.Core.Chunks;
using BrewUp.Knowledge.Core.Embeddings;

namespace BrewUp.Knowledge.Facade.Ingestion;

public interface IKnowledgeVectorStore
{
    Task StoreAsync(
        KnowledgeChunk chunk,
        EmbeddingVector embedding,
        CancellationToken cancellationToken);
}
