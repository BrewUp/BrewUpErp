using BrewUp.Knowledge.Core.Chunks;

namespace BrewUp.Knowledge.Core.Search;

public sealed class KnowledgeSearchResult
{
    public IReadOnlyCollection<KnowledgeSearchMatch> Matches { get; init; } = [];

    public IReadOnlyCollection<KnowledgeChunk> Chunks => Matches.Select(match => match.Chunk).ToArray();
}
