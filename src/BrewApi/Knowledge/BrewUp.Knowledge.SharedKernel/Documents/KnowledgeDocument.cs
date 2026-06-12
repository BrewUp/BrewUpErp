using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.SharedKernel.Documents;

public class KnowledgeDocument
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DocumentSource Source { get; init; } = DocumentSource.PlainText;
    public DocumentScope Scope { get; init; } = DocumentScope.General;
    public IReadOnlyCollection<string> Tags { get; init; } = [];
    public DateTime ImportedAt { get; init; }
}
