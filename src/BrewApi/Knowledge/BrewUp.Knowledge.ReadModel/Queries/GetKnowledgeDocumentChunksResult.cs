namespace BrewUp.Knowledge.ReadModel.Queries;

public sealed record GetKnowledgeDocumentChunksResult(
    Guid DocumentId,
    int ChunkCount,
    IReadOnlyCollection<KnowledgeDocumentChunkResult> Chunks);

public sealed record KnowledgeDocumentChunkResult(
    Guid Id,
    int Sequence,
    int TokenCount,
    string Content);
