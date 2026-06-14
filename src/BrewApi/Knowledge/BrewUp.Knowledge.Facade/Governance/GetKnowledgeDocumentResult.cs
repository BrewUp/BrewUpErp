namespace BrewUp.Knowledge.Facade.Governance;

public sealed record GetKnowledgeDocumentResult(
    Guid Id,
    string Title,
    string Scope,
    string Source,
    IReadOnlyCollection<string> Tags,
    DateTime ImportedAt,
    string Content,
    int ChunkCount);
