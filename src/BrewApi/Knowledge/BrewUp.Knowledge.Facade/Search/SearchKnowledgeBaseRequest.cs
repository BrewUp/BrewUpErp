using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Core.Search;

namespace BrewUp.Knowledge.Facade.Search;

public sealed class SearchKnowledgeBaseRequest
{
    public string Query { get; init; } = string.Empty;
    public DocumentScope? Scope { get; init; }
    public int MaxResults { get; init; } = 5;
    public double MinimumScore { get; init; }

    internal KnowledgeSearchRequest ToCoreRequest() => new()
    {
        Query = Query,
        Scope = Scope,
        MaxResults = MaxResults,
        MinimumScore = MinimumScore
    };
}

public sealed record SearchKnowledgeBaseMatch(
    Guid ChunkId,
    Guid DocumentId,
    string Title,
    string Content,
    int Sequence,
    double Score);

public sealed record SearchKnowledgeBaseResult(IReadOnlyCollection<SearchKnowledgeBaseMatch> Matches)
{
    internal static SearchKnowledgeBaseResult From(KnowledgeSearchResult result)
        => new(result.Matches
            .Select(match => new SearchKnowledgeBaseMatch(
                match.Chunk.Id,
                match.Chunk.DocumentId,
                match.Chunk.Metadata.Title,
                match.Chunk.Content,
                match.Chunk.Sequence,
                match.Score))
            .ToArray());
}
