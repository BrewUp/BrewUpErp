using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.SharedKernel.CustomTypes;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;

namespace BrewUp.Knowledge.Facade;

public interface IKnowledgeFacade
{
    Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeDocument command,
        CancellationToken cancellationToken);

    Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeFile command,
        CancellationToken cancellationToken);
}
