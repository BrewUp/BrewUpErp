using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Facade.Ingestion;

public sealed record IngestKnowledgeFileCommand(
    string FileName,
    Stream Content,
    DocumentScope Scope,
    IReadOnlyCollection<string>? Tags = null);
