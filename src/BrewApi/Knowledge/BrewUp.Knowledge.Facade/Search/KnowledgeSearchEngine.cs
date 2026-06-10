using BrewUp.Knowledge.Core.Embeddings;
using BrewUp.Knowledge.Core.Search;

namespace BrewUp.Knowledge.Facade.Search;

internal sealed class KnowledgeSearchEngine(
    IEmbeddingGenerator embeddingGenerator,
    IKnowledgeIndex knowledgeIndex) : IKnowledgeSearchEngine
{
    public async Task<KnowledgeSearchResult> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("A search query is required.", nameof(request));

        if (request.MaxResults is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(request), "MaxResults must be between 1 and 100.");

        if (request.MinimumScore is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(request), "MinimumScore must be between -1 and 1.");

        var queryEmbedding = await embeddingGenerator.GenerateAsync(request.Query, cancellationToken);
        var matches = await knowledgeIndex.SearchAsync(
            queryEmbedding,
            request.Scope,
            request.MaxResults,
            request.MinimumScore,
            cancellationToken);

        return new KnowledgeSearchResult { Matches = matches };
    }
}
