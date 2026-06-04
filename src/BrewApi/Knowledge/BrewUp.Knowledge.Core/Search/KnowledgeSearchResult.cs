using BrewUp.Knowledge.Core.Chunks;

namespace BrewUp.Knowledge.Core.Search;

public sealed class KnowledgeSearchResult
{
    public IReadOnlyCollection<KnowledgeChunk> Chunks { get; init; }
        = [];
}