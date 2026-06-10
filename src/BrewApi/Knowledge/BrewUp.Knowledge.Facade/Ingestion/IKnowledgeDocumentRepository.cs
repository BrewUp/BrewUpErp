using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Facade.Ingestion;

public interface IKnowledgeDocumentRepository
{
    Task StoreAsync(KnowledgeDocument document, CancellationToken cancellationToken);
}
