using BrewUp.Knowledge.Core.Chunks;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Core.Embeddings;

namespace BrewUp.Knowledge.Core.Search;

public interface IKnowledgeIndex
{
    Task ReplaceDocumentAsync(
        Guid documentId,
        IReadOnlyCollection<EmbeddedKnowledgeChunk> chunks,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<KnowledgeSearchMatch>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int maxResults,
        double minimumScore,
        CancellationToken cancellationToken);
}

public sealed record EmbeddedKnowledgeChunk(KnowledgeChunk Chunk, EmbeddingVector Embedding);
