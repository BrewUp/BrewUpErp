using System.Collections.Concurrent;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Facade.Ingestion;

namespace BrewUp.Knowledge.Infrastructure;

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

    public bool TryGet(Guid documentId, out KnowledgeDocument? document)
        => _documents.TryGetValue(documentId, out document);
}
