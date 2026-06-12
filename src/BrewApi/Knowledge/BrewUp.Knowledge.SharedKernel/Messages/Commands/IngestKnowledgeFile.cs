using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.Facade.Ingestion;

public sealed record IngestKnowledgeFile(
    string FileName,
    Stream Content,
    DocumentScope Scope,
    IReadOnlyCollection<string>? Tags = null);
