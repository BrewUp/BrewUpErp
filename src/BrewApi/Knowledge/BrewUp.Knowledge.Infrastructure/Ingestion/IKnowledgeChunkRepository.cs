using BrewUp.Knowledge.SharedKernel.Chunks;

namespace BrewUp.Knowledge.Infrastructure.Ingestion;

public interface IKnowledgeChunkRepository
{
    Task<IReadOnlyCollection<KnowledgeChunk>> GetByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken);
}
