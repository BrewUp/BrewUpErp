using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Core.Chunks;

public sealed class ChunkMetadata
{
    public DocumentScope Scope { get; init; }
    public string Title { get; init; } = string.Empty;
    public int TokenCount { get; init; }
}