namespace BrewUp.Knowledge.SharedKernel.Chunks;

public class KnowledgeChunk
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public string KnowledgeContent { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public ChunkMetadata Metadata { get; init; } = null!;
}