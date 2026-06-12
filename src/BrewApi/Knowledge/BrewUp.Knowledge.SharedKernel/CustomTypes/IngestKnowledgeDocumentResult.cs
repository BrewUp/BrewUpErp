namespace BrewUp.Knowledge.SharedKernel.CustomTypes;

public sealed record IngestKnowledgeDocumentResult(Guid DocumentId, int ChunkCount);
