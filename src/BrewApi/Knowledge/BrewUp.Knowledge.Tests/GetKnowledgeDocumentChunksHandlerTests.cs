using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Chunks;

namespace BrewUp.Knowledge.Tests;

public sealed class GetKnowledgeDocumentChunksHandlerTests
{
    [Fact]
    public async Task DocumentWithChunks_ReturnsAllChunksOrderedBySequence()
    {
        var documentId = Guid.NewGuid();
        var repository = new InMemoryKnowledgeChunkRepository();
        var chunks = new[]
        {
            CreateChunk(documentId, sequence: 2, tokenCount: 30),
            CreateChunk(documentId, sequence: 0, tokenCount: 10),
            CreateChunk(documentId, sequence: 1, tokenCount: 20)
        };
        await ((IKnowledgeChunkWriter)repository).StoreAsync(chunks, CancellationToken.None);
        var handler = new GetKnowledgeDocumentChunksHandler(repository);

        var result = await handler.HandleAsync(
            new GetKnowledgeDocumentChunksQuery(documentId),
            CancellationToken.None);

        Assert.Equal(documentId, result.DocumentId);
        Assert.Equal(chunks.Length, result.Chunks.Count);
        Assert.Equal(new[] { 0, 1, 2 }, result.Chunks.Select(chunk => chunk.Sequence));
        Assert.Equal(chunks.Length, result.ChunkCount);
    }

    [Fact]
    public async Task UnknownDocumentId_ReturnsEmptyCollection()
    {
        var handler = new GetKnowledgeDocumentChunksHandler(
            new InMemoryKnowledgeChunkRepository());
        var documentId = Guid.NewGuid();

        var result = await handler.HandleAsync(
            new GetKnowledgeDocumentChunksQuery(documentId),
            CancellationToken.None);

        Assert.Equal(documentId, result.DocumentId);
        Assert.Equal(0, result.ChunkCount);
        Assert.Empty(result.Chunks);
    }

    private static KnowledgeChunk CreateChunk(
        Guid documentId,
        int sequence,
        int tokenCount)
        => new()
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Sequence = sequence,
            Content = $"Chunk {sequence}",
            Metadata = new ChunkMetadata
            {
                TokenCount = tokenCount
            }
        };
}
