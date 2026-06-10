using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Core.Embeddings;
using BrewUp.Knowledge.Core.Search;

namespace BrewUp.Knowledge.Facade.Ingestion;

internal sealed class KnowledgeIngestionService(
    IChunkingStrategy chunkingStrategy,
    IEmbeddingGenerator embeddingGenerator,
    IKnowledgeIndex knowledgeIndex) : IKnowledgeIngestionService
{
    public async Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("A document title is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Document content is required.", nameof(request));

        var document = new KnowledgeDocument
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Content = request.Content,
            Source = request.Source,
            Scope = request.Scope,
            ImportedAt = DateTime.UtcNow
        };

        var chunks = chunkingStrategy.Split(document);
        var embeddedChunks = new List<EmbeddedKnowledgeChunk>(chunks.Count);

        foreach (var chunk in chunks)
        {
            var embedding = await embeddingGenerator.GenerateAsync(chunk.Content, cancellationToken);
            embeddedChunks.Add(new EmbeddedKnowledgeChunk(chunk, embedding));
        }

        await knowledgeIndex.ReplaceDocumentAsync(document.Id, embeddedChunks, cancellationToken);
        return new IngestKnowledgeDocumentResult(document.Id, embeddedChunks.Count);
    }
}
