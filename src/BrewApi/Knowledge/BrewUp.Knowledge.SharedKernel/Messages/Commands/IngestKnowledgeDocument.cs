using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.SharedKernel.Messages.Commands;

public sealed record IngestKnowledgeDocument(
    string Title,
    string Content,
    DocumentScope Scope,
    DocumentSource Source,
    IReadOnlyCollection<string>? Tags = null);
