namespace BrewUp.Knowledge.Facade.Governance;

public sealed record GetKnowledgeDocumentsResult(
    IReadOnlyCollection<KnowledgeDocumentSummary> Documents);

public sealed record KnowledgeDocumentSummary(
    Guid Id,
    string Title,
    string Scope,
    string Source,
    IReadOnlyCollection<string> Tags,
    DateTime ImportedAt,
    int ChunkCount);
