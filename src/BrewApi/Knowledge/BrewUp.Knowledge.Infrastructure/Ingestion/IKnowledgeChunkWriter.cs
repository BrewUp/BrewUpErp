using BrewUp.Knowledge.SharedKernel.Chunks;

namespace BrewUp.Knowledge.Infrastructure.Ingestion;

public interface IKnowledgeChunkWriter
{
    Task StoreAsync(
        IReadOnlyCollection<KnowledgeChunk> chunks,
        CancellationToken cancellationToken);
}
