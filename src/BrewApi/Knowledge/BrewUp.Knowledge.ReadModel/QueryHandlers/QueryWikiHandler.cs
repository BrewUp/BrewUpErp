using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.ReadModel.QueryHandlers;

public sealed class QueryWikiHandler(
    IEmbeddingGenerator embeddingGenerator,
    IWikiRepository wikiRepository)
{
    public async Task<WikiSearchResult> HandleAsync(
        QueryWiki query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.Query))
            throw new ArgumentException("A Wiki query is required.", nameof(query.Query));

        var topK = query.TopK ?? 5;
        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(query.TopK), "TopK must be greater than zero.");

        var scope = string.IsNullOrWhiteSpace(query.Scope)
            ? null
            : DocumentScope.FromName(query.Scope.Trim());
        var embedding = await embeddingGenerator.GenerateAsync(
            query.Query.Trim(),
            cancellationToken);
        var matches = await wikiRepository.SearchAsync(
            embedding,
            scope,
            Math.Min(topK, 20),
            cancellationToken);
        return new WikiSearchResult(matches.Select(match =>
            new WikiSearchResultItem(
                match.Page.Id,
                match.Page.NormalizedKey,
                match.Page.Title,
                match.Page.PageType,
                match.Page.Scope.Name,
                match.Page.CurrentRevision,
                match.Page.Content,
                match.Score)).ToArray());
    }
}

