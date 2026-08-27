using BrewUp.Knowledge.SharedKernel.Wiki;

public sealed record ReindexKnowledgeDocumentResult(
    Guid DocumentId,
    int ChunkCount,
    WikiProcessingStatus WikiStatus = WikiProcessingStatus.Disabled);
