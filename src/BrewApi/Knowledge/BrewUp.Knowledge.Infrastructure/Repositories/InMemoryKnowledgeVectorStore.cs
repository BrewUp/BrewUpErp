using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

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

    public Task<IReadOnlyCollection<KnowledgeVectorSearchResult>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int topK,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topK);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            IReadOnlyCollection<KnowledgeVectorSearchResult> results = _vectors
                .Where(item => scope is null || item.Chunk.Metadata.Scope.Equals(scope))
                .Select(item => new KnowledgeVectorSearchResult(
                    item.Chunk,
                    CosineSimilarity(queryEmbedding, item.Embedding)))
                .OrderByDescending(result => result.Score)
                .ThenBy(result => result.Chunk.Sequence)
                .ThenBy(result => result.Chunk.Id)
                .Take(topK)
                .ToArray();

            return Task.FromResult(results);
        }
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

    private static double CosineSimilarity(
        EmbeddingVector left,
        EmbeddingVector right)
    {
        if (left.Dimensions != right.Dimensions)
            throw new InvalidOperationException(
                "Cannot compare embedding vectors with different dimensions.");

        double dotProduct = 0;
        double leftMagnitudeSquared = 0;
        double rightMagnitudeSquared = 0;

        for (var index = 0; index < left.Dimensions; index++)
        {
            var leftValue = left.Values[index];
            var rightValue = right.Values[index];

            dotProduct += leftValue * rightValue;
            leftMagnitudeSquared += leftValue * leftValue;
            rightMagnitudeSquared += rightValue * rightValue;
        }

        if (leftMagnitudeSquared == 0 || rightMagnitudeSquared == 0)
            return 0;

        return dotProduct /
               (Math.Sqrt(leftMagnitudeSquared) * Math.Sqrt(rightMagnitudeSquared));
    }
}
