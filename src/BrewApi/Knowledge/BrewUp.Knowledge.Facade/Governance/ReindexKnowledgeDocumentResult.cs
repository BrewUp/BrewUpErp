namespace BrewUp.Knowledge.Facade.Governance;

public sealed record ReindexKnowledgeDocumentResult(
    Guid DocumentId,
    int ChunkCount);
