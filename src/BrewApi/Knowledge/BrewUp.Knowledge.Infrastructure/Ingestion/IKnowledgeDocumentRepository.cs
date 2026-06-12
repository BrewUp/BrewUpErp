using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.Infrastructure.Ingestion;

public interface IKnowledgeDocumentRepository
{
    Task StoreAsync(KnowledgeDocument document, CancellationToken cancellationToken);
}
