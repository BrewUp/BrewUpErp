using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.SharedKernel.Chunks;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class InMemoryKnowledgeChunkRepository :
    IKnowledgeChunkRepository,
    IKnowledgeChunkWriter
{
    private readonly Lock _lock = new();
    private IReadOnlyList<KnowledgeChunk> _chunks = [];

    public Task StoreAsync(
        IReadOnlyCollection<KnowledgeChunk> chunks,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chunks);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var chunkIds = chunks.Select(chunk => chunk.Id).ToHashSet();
            _chunks = _chunks
                .Where(chunk => !chunkIds.Contains(chunk.Id))
                .Concat(chunks)
                .ToArray();
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<KnowledgeChunk>> GetByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            IReadOnlyCollection<KnowledgeChunk> chunks = _chunks
                .Where(chunk => chunk.DocumentId == documentId)
                .ToArray();

            return Task.FromResult(chunks);
        }
    }

    public Task<int> CountByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
            return Task.FromResult(_chunks.Count(chunk => chunk.DocumentId == documentId));
    }

    public Task DeleteByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
            _chunks = _chunks.Where(chunk => chunk.DocumentId != documentId).ToArray();

        return Task.CompletedTask;
    }
}
