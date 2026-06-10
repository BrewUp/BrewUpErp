using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Facade.Ingestion;

public sealed class IngestKnowledgeDocumentRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DocumentSource Source { get; init; } = DocumentSource.PlainText;
    public DocumentScope Scope { get; init; } = DocumentScope.General;
}

public sealed record IngestKnowledgeDocumentResult(Guid DocumentId, int ChunkCount);
