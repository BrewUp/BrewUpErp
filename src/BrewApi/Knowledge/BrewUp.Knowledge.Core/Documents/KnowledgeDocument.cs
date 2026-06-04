namespace BrewUp.Knowledge.Core.Documents;

public class KnowledgeDocument
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DocumentSource Source { get; init; }
    public DocumentScope Scope { get; init; }
    public DateTime ImportedAt { get; init; }
}