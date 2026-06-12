using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.ReadModel.QueryHandlers;

public sealed class SearchKnowledgeHandler(
    IEmbeddingGenerator embeddingGenerator,
    IKnowledgeVectorStore vectorStore)
{
    public const int DefaultTopK = 5;
    public const int MaximumTopK = 20;

    public async Task<SearchKnowledgeResult> HandleAsync(
        SearchKnowledgeQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.Query))
            throw new ArgumentException("A search query is required.", nameof(query.Query));

        var scope = ParseScope(query.Scope);
        var topK = NormalizeTopK(query.TopK);
        var queryEmbedding = await embeddingGenerator.GenerateAsync(
            query.Query.Trim(),
            cancellationToken);
        var matches = await vectorStore.SearchAsync(
            queryEmbedding,
            scope,
            topK,
            cancellationToken);

        var items = matches
            .Select(match => new KnowledgeSearchResultItem(
                match.Chunk.DocumentId,
                match.Chunk.Id,
                match.Chunk.Sequence,
                match.Chunk.Metadata.Title,
                match.Chunk.Metadata.Scope.Name,
                match.Chunk.Metadata.Tags,
                match.Chunk.Content,
                match.Score,
                match.Chunk.Metadata.TokenCount))
            .ToArray();

        return new SearchKnowledgeResult(items);
    }

    private static int NormalizeTopK(int? topK)
    {
        var requestedTopK = topK ?? DefaultTopK;
        if (requestedTopK <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(topK),
                "TopK must be greater than zero.");

        return Math.Min(requestedTopK, MaximumTopK);
    }

    private static DocumentScope? ParseScope(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
            return null;

        try
        {
            return DocumentScope.FromName(scope.Trim());
        }
        catch (Exception exception)
        {
            throw new ArgumentException(exception.Message, nameof(scope), exception);
        }
    }
}