using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using BrewUp.Knowledge.SharedKernel.Configuration;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class AzureAiSearchIndexInitializer(
    SearchIndexClient searchIndexClient,
    AzureAiSearchOptions options)
{
    private const string HnswAlgorithmName = "knowledge-hnsw";
    private const string VectorProfileName = "knowledge-vector-profile";
    private const int VectorDimensions = 1536;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.IndexName))
            throw new InvalidOperationException(
                $"{AzureAiSearchOptions.SectionName}:IndexName is required.");

        try
        {
            await searchIndexClient.GetIndexAsync(
                options.IndexName,
                cancellationToken);
            return;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            await searchIndexClient.CreateIndexAsync(
                CreateIndex(options.IndexName),
                cancellationToken);
        }
    }

    internal static SearchIndex CreateIndex(string indexName)
    {
        var fields = new[]
        {
            new SearchField("id", SearchFieldDataType.String)
            {
                IsKey = true
            },
            FilterableString("chunkId"),
            FilterableString("documentId"),
            new SearchField("sequence", SearchFieldDataType.Int32)
            {
                IsFilterable = true,
                IsSortable = true
            },
            new SearchField("title", SearchFieldDataType.String)
            {
                IsSearchable = true,
                IsFilterable = true
            },
            FilterableString("scope"),
            new SearchField(
                "tags",
                SearchFieldDataType.Collection(SearchFieldDataType.String))
            {
                IsFilterable = true
            },
            new SearchField("content", SearchFieldDataType.String)
            {
                IsSearchable = true
            },
            FilterableInt32("tokenCount"),
            FilterableInt32("maxCharacters"),
            FilterableInt32("overlapCharacters"),
            new SearchField(
                "embedding",
                SearchFieldDataType.Collection(SearchFieldDataType.Single))
            {
                VectorSearchDimensions = VectorDimensions,
                VectorSearchProfileName = VectorProfileName
            }
        };

        return new SearchIndex(indexName, fields)
        {
            VectorSearch = new VectorSearch
            {
                Algorithms =
                {
                    new HnswAlgorithmConfiguration(HnswAlgorithmName)
                },
                Profiles =
                {
                    new VectorSearchProfile(
                        VectorProfileName,
                        HnswAlgorithmName)
                }
            }
        };
    }

    private static SearchField FilterableString(string name)
        => new(name, SearchFieldDataType.String)
        {
            IsFilterable = true
        };

    private static SearchField FilterableInt32(string name)
        => new(name, SearchFieldDataType.Int32)
        {
            IsFilterable = true
        };
}
