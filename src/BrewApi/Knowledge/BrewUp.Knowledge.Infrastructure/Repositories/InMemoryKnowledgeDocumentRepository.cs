using System.Collections.Concurrent;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class InMemoryKnowledgeDocumentRepository : IKnowledgeDocumentRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeDocument> _documents = new();

    public Task StoreAsync(KnowledgeDocument document, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<KnowledgeDocument>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<KnowledgeDocument> documents = _documents.Values
            .OrderByDescending(document => document.ImportedAt)
            .ThenBy(document => document.Title)
            .ThenBy(document => document.Id)
            .ToArray();

        return Task.FromResult(documents);
    }

    public Task<KnowledgeDocument?> GetByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _documents.TryGetValue(documentId, out var document);
        return Task.FromResult(document);
    }

    public Task<bool> DeleteAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_documents.TryRemove(documentId, out _));
    }

    public bool TryGet(Guid documentId, out KnowledgeDocument? document)
        => _documents.TryGetValue(documentId, out document);
}
