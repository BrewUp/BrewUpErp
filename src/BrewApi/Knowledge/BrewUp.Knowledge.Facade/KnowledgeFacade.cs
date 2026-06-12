using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.SharedKernel.CustomTypes;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;

namespace BrewUp.Knowledge.Facade;

internal sealed class KnowledgeFacade(
    IngestKnowledgeDocumentHandler ingestionHandler) : IKnowledgeFacade
{
    public Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeDocument command,
        CancellationToken cancellationToken)
        => ingestionHandler.HandleAsync(command, cancellationToken);

    public Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeFile command,
        CancellationToken cancellationToken)
        => ingestionHandler.HandleAsync(command, cancellationToken);
}