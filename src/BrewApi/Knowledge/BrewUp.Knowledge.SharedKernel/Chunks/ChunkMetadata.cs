using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.SharedKernel.Chunks;

public sealed class ChunkMetadata
{
    public DocumentScope Scope { get; init; } = DocumentScope.General;
    public string Title { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Tags { get; init; } = [];
    public int TokenCount { get; init; }
    public int MaxCharacters { get; init; }
    public int OverlapCharacters { get; init; }
}
