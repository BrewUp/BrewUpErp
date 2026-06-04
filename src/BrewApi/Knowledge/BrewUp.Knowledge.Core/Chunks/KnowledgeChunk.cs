namespace BrewUp.Knowledge.Core.Chunks;

public class KnowledgeChunk
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public ChunkMetadata Metadata { get; init; } = default!;
}