using BrewUp.Knowledge.Facade.Ingestion;

namespace BrewUp.Knowledge.Facade;

public interface IKnowledgeFacade
{
    Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeDocumentCommand command,
        CancellationToken cancellationToken);

    Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeFileCommand command,
        CancellationToken cancellationToken);
}
