using BrewUp.Knowledge.Core.Chunks;
using BrewUp.Knowledge.Core.Embeddings;
using BrewUp.Knowledge.Facade.Ingestion;

namespace BrewUp.Knowledge.Infrastructure;

public sealed class InMemoryKnowledgeVectorStore : IKnowledgeVectorStore
{
    private readonly Lock _lock = new();
    private IReadOnlyList<StoredKnowledgeVector> _vectors = [];

    public Task StoreAsync(
        KnowledgeChunk chunk,
        EmbeddingVector embedding,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(embedding);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            _vectors = _vectors
                .Where(item => item.Chunk.Id != chunk.Id)
                .Append(new StoredKnowledgeVector(chunk, embedding))
                .ToArray();
        }

        return Task.CompletedTask;
    }

    internal int Count
    {
        get
        {
            lock (_lock)
                return _vectors.Count;
        }
    }

    private sealed record StoredKnowledgeVector(
        KnowledgeChunk Chunk,
        EmbeddingVector Embedding);
}
