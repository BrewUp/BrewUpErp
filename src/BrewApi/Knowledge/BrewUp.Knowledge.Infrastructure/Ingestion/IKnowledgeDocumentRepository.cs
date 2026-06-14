using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.Infrastructure.Ingestion;

public interface IKnowledgeDocumentRepository
{
    Task StoreAsync(KnowledgeDocument document, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<KnowledgeDocument>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<KnowledgeDocument?> GetByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid documentId,
        CancellationToken cancellationToken);
}
