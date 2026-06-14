using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class AzureAiSearchKnowledgeVectorStore(
    SearchClient searchClient,
    AzureAiSearchIndexInitializer indexInitializer,
    AzureAiSearchOptions options) : IKnowledgeVectorStore
{
    private const int MaximumIndexBatchSize = 1000;

    public SearchClient SearchClient { get; } = searchClient;
    public AzureAiSearchIndexInitializer IndexInitializer { get; } = indexInitializer;
    public AzureAiSearchOptions Options { get; } = options;

    public async Task StoreAsync(
        KnowledgeChunk chunk,
        EmbeddingVector embedding,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(embedding);

        if (embedding.Dimensions != Options.Dimensions)
            throw new ArgumentException(
                $"Embedding must contain {Options.Dimensions} dimensions.",
                nameof(embedding));

        await IndexInitializer.InitializeAsync(cancellationToken);

        await SearchClient.MergeOrUploadDocumentsAsync(
            new[] { CreateSearchDocument(chunk, embedding) },
            new IndexDocumentsOptions
            {
                ThrowOnAnyError = true
            },
            cancellationToken);
    }

    internal static SearchDocument CreateSearchDocument(
        KnowledgeChunk chunk,
        EmbeddingVector embedding)
    {
        ArgumentNullException.ThrowIfNull(chunk.Metadata);

        return new SearchDocument
        {
            ["id"] = chunk.Id.ToString("D"),
            ["chunkId"] = chunk.Id.ToString("D"),
            ["documentId"] = chunk.DocumentId.ToString("D"),
            ["sequence"] = chunk.Sequence,
            ["title"] = chunk.Metadata.Title,
            ["scope"] = chunk.Metadata.Scope.Name,
            ["tags"] = chunk.Metadata.Tags.ToArray(),
            ["content"] = chunk.KnowledgeContent,
            ["tokenCount"] = chunk.Metadata.TokenCount,
            ["maxCharacters"] = chunk.Metadata.MaxCharacters,
            ["overlapCharacters"] = chunk.Metadata.OverlapCharacters,
            ["embedding"] = embedding.Values.ToArray()
        };
    }

    public async Task<IReadOnlyCollection<KnowledgeVectorSearchResult>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int topK,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topK);

        if (queryEmbedding.Dimensions != Options.Dimensions)
            throw new ArgumentException(
                $"Embedding must contain {Options.Dimensions} dimensions.",
                nameof(queryEmbedding));

        await IndexInitializer.InitializeAsync(cancellationToken);

        var response = await SearchClient.SearchAsync<SearchDocument>(
            CreateSearchOptions(queryEmbedding, scope, topK),
            cancellationToken);
        var results = new List<KnowledgeVectorSearchResult>();

        await foreach (var result in response.Value.GetResultsAsync())
            results.Add(CreateVectorSearchResult(
                result.Document,
                result.Score ?? 0));

        return results
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Chunk.Sequence)
            .ThenBy(result => result.Chunk.Id)
            .ToArray();
    }

    internal static SearchOptions CreateSearchOptions(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int topK)
    {
        var vectorQuery = new VectorizedQuery(
            queryEmbedding.Values.ToArray())
        {
            KNearestNeighborsCount = topK
        };
        vectorQuery.Fields.Add("embedding");

        var options = new SearchOptions
        {
            Size = topK,
            Filter = scope is null
                ? null
                : SearchFilter.Create($"scope eq {scope.Name}"),
            VectorSearch = new VectorSearchOptions()
        };

        options.VectorSearch.Queries.Add(vectorQuery);
        options.Select.Add("chunkId");
        options.Select.Add("documentId");
        options.Select.Add("sequence");
        options.Select.Add("title");
        options.Select.Add("scope");
        options.Select.Add("tags");
        options.Select.Add("content");
        options.Select.Add("tokenCount");
        options.Select.Add("maxCharacters");
        options.Select.Add("overlapCharacters");

        return options;
    }

    internal static KnowledgeVectorSearchResult CreateVectorSearchResult(
        SearchDocument document,
        double score)
    {
        var chunk = new KnowledgeChunk
        {
            Id = Guid.Parse(GetRequiredString(document, "chunkId")),
            DocumentId = Guid.Parse(GetRequiredString(document, "documentId")),
            Sequence = GetRequiredInt32(document, "sequence"),
            KnowledgeContent = GetRequiredString(document, "content"),
            Metadata = new ChunkMetadata
            {
                Title = GetRequiredString(document, "title"),
                Scope = DocumentScope.FromName(
                    GetRequiredString(document, "scope")),
                Tags = GetStringCollection(document, "tags"),
                TokenCount = GetRequiredInt32(document, "tokenCount"),
                MaxCharacters = GetRequiredInt32(
                    document,
                    "maxCharacters"),
                OverlapCharacters = GetRequiredInt32(
                    document,
                    "overlapCharacters")
            }
        };

        return new KnowledgeVectorSearchResult(chunk, score);
    }

    private static string GetRequiredString(
        SearchDocument document,
        string fieldName)
    {
        if (!document.TryGetValue(fieldName, out var value) ||
            value is null)
            throw new InvalidOperationException(
                $"Azure AI Search result is missing '{fieldName}'.");

        return Convert.ToString(value)
               ?? throw new InvalidOperationException(
                   $"Azure AI Search field '{fieldName}' is invalid.");
    }

    private static int GetRequiredInt32(
        SearchDocument document,
        string fieldName)
    {
        if (!document.TryGetValue(fieldName, out var value) ||
            value is null)
            throw new InvalidOperationException(
                $"Azure AI Search result is missing '{fieldName}'.");

        return Convert.ToInt32(value);
    }

    private static IReadOnlyCollection<string> GetStringCollection(
        SearchDocument document,
        string fieldName)
    {
        if (!document.TryGetValue(fieldName, out var value) ||
            value is null)
            return [];

        if (value is IEnumerable<string> strings)
            return strings.ToArray();

        if (value is IEnumerable<object> objects)
            return objects
                .Select(Convert.ToString)
                .Where(item => item is not null)
                .Cast<string>()
                .ToArray();

        throw new InvalidOperationException(
            $"Azure AI Search field '{fieldName}' is invalid.");
    }

    public async Task DeleteByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await IndexInitializer.InitializeAsync(cancellationToken);

        var response = await SearchClient.SearchAsync<SearchDocument>(
            "*",
            CreateDeleteSearchOptions(documentId),
            cancellationToken);
        var keys = new List<string>();

        await foreach (var result in response.Value.GetResultsAsync())
            keys.Add(GetRequiredString(result.Document, "id"));

        foreach (var batch in BatchDocumentKeys(keys))
            await SearchClient.DeleteDocumentsAsync(
                "id",
                batch,
                new IndexDocumentsOptions
                {
                    ThrowOnAnyError = true
                },
                cancellationToken);
    }

    internal static SearchOptions CreateDeleteSearchOptions(Guid documentId)
    {
        var documentIdValue = documentId.ToString("D");
        var options = new SearchOptions
        {
            Filter = SearchFilter.Create(
                $"documentId eq {documentIdValue}"),
            Size = MaximumIndexBatchSize
        };
        options.Select.Add("id");

        return options;
    }

    internal static IReadOnlyList<IReadOnlyCollection<string>>
        BatchDocumentKeys(IEnumerable<string> keys)
        => keys
            .Chunk(MaximumIndexBatchSize)
            .Select(batch => (IReadOnlyCollection<string>)batch)
            .ToArray();
}
