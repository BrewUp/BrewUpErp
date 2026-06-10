using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Facade.Ingestion;

public sealed record IngestKnowledgeDocumentCommand(
    string Title,
    string Content,
    DocumentScope Scope,
    DocumentSource Source,
    IReadOnlyCollection<string>? Tags = null);
