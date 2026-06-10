using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Core.Embeddings;
using BrewUp.Knowledge.Core.Search;

namespace BrewUp.Knowledge.Infrastructure;

internal sealed class InMemoryKnowledgeIndex : IKnowledgeIndex
{
    private readonly Lock _lock = new();
    private IReadOnlyList<EmbeddedKnowledgeChunk> _chunks = [];

    public Task ReplaceDocumentAsync(
        Guid documentId,
        IReadOnlyCollection<EmbeddedKnowledgeChunk> chunks,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (chunks.Any(item => item.Chunk.DocumentId != documentId))
            throw new ArgumentException("All chunks must belong to the supplied document.", nameof(chunks));

        lock (_lock)
        {
            _chunks = _chunks
                .Where(item => item.Chunk.DocumentId != documentId)
                .Concat(chunks)
                .ToArray();
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<KnowledgeSearchMatch>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int maxResults,
        double minimumScore,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<EmbeddedKnowledgeChunk> snapshot;
        lock (_lock)
            snapshot = _chunks;

        var matches = snapshot
            .Where(item => scope is null ||
                           string.Equals(
                               item.Chunk.Metadata.Scope.Name,
                               scope.Name,
                               StringComparison.OrdinalIgnoreCase))
            .Select(item => new KnowledgeSearchMatch(
                item.Chunk,
                CosineSimilarity(queryEmbedding.Values, item.Embedding.Values)))
            .Where(match => match.Score >= minimumScore)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Chunk.DocumentId)
            .ThenBy(match => match.Chunk.Sequence)
            .Take(maxResults)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<KnowledgeSearchMatch>>(matches);
    }

    private static double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        if (left.Count == 0 || left.Count != right.Count)
            return 0;

        double dotProduct = 0;
        double leftMagnitude = 0;
        double rightMagnitude = 0;

        for (var i = 0; i < left.Count; i++)
        {
            dotProduct += left[i] * right[i];
            leftMagnitude += left[i] * left[i];
            rightMagnitude += right[i] * right[i];
        }

        if (leftMagnitude == 0 || rightMagnitude == 0)
            return 0;

        return dotProduct / (Math.Sqrt(leftMagnitude) * Math.Sqrt(rightMagnitude));
    }
}
