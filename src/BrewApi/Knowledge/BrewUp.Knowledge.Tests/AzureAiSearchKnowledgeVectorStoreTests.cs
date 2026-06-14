using Azure.Search.Documents.Models;
using BrewUp.Knowledge.Infrastructure.Repositories;
using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.Tests;

public sealed class AzureAiSearchKnowledgeVectorStoreTests
{
    [Fact]
    public void CreateSearchDocument_MapsChunkAndEmbeddingToRetrievalProjection()
    {
        var chunkId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var chunk = new KnowledgeChunk
        {
            Id = chunkId,
            DocumentId = documentId,
            Sequence = 3,
            KnowledgeContent = "Contact time between 3 and 7 days.",
            Metadata = new ChunkMetadata
            {
                Title = "IPA Brewing Guide",
                Scope = DocumentScope.Production,
                Tags = ["ipa", "dry-hopping"],
                TokenCount = 9,
                MaxCharacters = 1200,
                OverlapCharacters = 150
            }
        };
        var embeddingValues = Enumerable.Range(0, 1536)
            .Select(index => index / 1536f)
            .ToArray();

        var document =
            AzureAiSearchKnowledgeVectorStore.CreateSearchDocument(
                chunk,
                new EmbeddingVector(embeddingValues));

        Assert.Equal(12, document.Count);
        Assert.Equal(chunkId.ToString("D"), document["id"]);
        Assert.Equal(chunkId.ToString("D"), document["chunkId"]);
        Assert.Equal(documentId.ToString("D"), document["documentId"]);
        Assert.Equal(3, document["sequence"]);
        Assert.Equal("IPA Brewing Guide", document["title"]);
        Assert.Equal("production", document["scope"]);
        Assert.Equal(
            ["ipa", "dry-hopping"],
            Assert.IsType<string[]>(document["tags"]));
        Assert.Equal(
            "Contact time between 3 and 7 days.",
            document["content"]);
        Assert.Equal(9, document["tokenCount"]);
        Assert.Equal(1200, document["maxCharacters"]);
        Assert.Equal(150, document["overlapCharacters"]);
        Assert.Equal(
            embeddingValues,
            Assert.IsType<float[]>(document["embedding"]));
    }

    [Fact]
    public void CreateSearchOptions_ConfiguresVectorSearchScopeAndProjection()
    {
        var embedding = new EmbeddingVector(new float[1536]);

        var options = AzureAiSearchKnowledgeVectorStore.CreateSearchOptions(
            embedding,
            DocumentScope.Production,
            5);

        Assert.Equal(5, options.Size);
        Assert.Equal("scope eq 'production'", options.Filter);
        Assert.DoesNotContain("embedding", options.Select);
        Assert.Equal(
            [
                "chunkId",
                "documentId",
                "sequence",
                "title",
                "scope",
                "tags",
                "content",
                "tokenCount",
                "maxCharacters",
                "overlapCharacters"
            ],
            options.Select);

        var vectorQuery = Assert.IsType<VectorizedQuery>(
            Assert.Single(options.VectorSearch.Queries));
        Assert.Equal(5, vectorQuery.KNearestNeighborsCount);
        Assert.Equal(["embedding"], vectorQuery.Fields);
        Assert.Equal(embedding.Values, vectorQuery.Vector.ToArray());
    }

    [Fact]
    public void CreateVectorSearchResult_MapsProjectionWithoutEmbedding()
    {
        var chunkId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var document = new SearchDocument
        {
            ["chunkId"] = chunkId.ToString("D"),
            ["documentId"] = documentId.ToString("D"),
            ["sequence"] = 2,
            ["title"] = "IPA Brewing Guide",
            ["scope"] = "production",
            ["tags"] = new[] { "ipa", "fermentation" },
            ["content"] = "Ferment at the recommended temperature.",
            ["tokenCount"] = 7,
            ["maxCharacters"] = 1200,
            ["overlapCharacters"] = 150
        };

        var result =
            AzureAiSearchKnowledgeVectorStore.CreateVectorSearchResult(
                document,
                0.93);

        Assert.Equal(chunkId, result.Chunk.Id);
        Assert.Equal(documentId, result.Chunk.DocumentId);
        Assert.Equal(2, result.Chunk.Sequence);
        Assert.Equal(
            "Ferment at the recommended temperature.",
            result.Chunk.KnowledgeContent);
        Assert.Equal("IPA Brewing Guide", result.Chunk.Metadata.Title);
        Assert.Equal(DocumentScope.Production, result.Chunk.Metadata.Scope);
        Assert.Equal(["ipa", "fermentation"], result.Chunk.Metadata.Tags);
        Assert.Equal(7, result.Chunk.Metadata.TokenCount);
        Assert.Equal(1200, result.Chunk.Metadata.MaxCharacters);
        Assert.Equal(150, result.Chunk.Metadata.OverlapCharacters);
        Assert.Equal(0.93, result.Score);
    }

    [Fact]
    public void CreateDeleteSearchOptions_FiltersDocumentAndSelectsOnlyKeys()
    {
        var documentId = Guid.NewGuid();

        var options =
            AzureAiSearchKnowledgeVectorStore.CreateDeleteSearchOptions(
                documentId);

        Assert.Equal(
            $"documentId eq '{documentId:D}'",
            options.Filter);
        Assert.Equal(1000, options.Size);
        Assert.Equal(["id"], options.Select);
    }

    [Fact]
    public void BatchDocumentKeys_UsesAzureBatchLimit()
    {
        var keys = Enumerable.Range(0, 1001)
            .Select(index => index.ToString())
            .ToArray();

        var batches =
            AzureAiSearchKnowledgeVectorStore.BatchDocumentKeys(keys);

        Assert.Equal(2, batches.Count);
        Assert.Equal(1000, batches[0].Count);
        Assert.Single(batches[1]);
    }
}
