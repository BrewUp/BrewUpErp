using BrewUp.Knowledge.Facade.Ingestion;

namespace BrewUp.Knowledge.Facade;

internal sealed class KnowledgeFacade(
    IngestKnowledgeDocumentHandler ingestionHandler) : IKnowledgeFacade
{
    public Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeDocumentCommand command,
        CancellationToken cancellationToken)
        => ingestionHandler.HandleAsync(command, cancellationToken);

    public Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeFileCommand command,
        CancellationToken cancellationToken)
        => ingestionHandler.HandleAsync(command, cancellationToken);
}