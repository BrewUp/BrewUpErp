using Azure.Search.Documents.Indexes.Models;
using BrewUp.Knowledge.Infrastructure.Repositories;

namespace BrewUp.Knowledge.Tests;

public sealed class AzureAiSearchIndexInitializerTests
{
    [Fact]
    public void CreateIndex_DefinesExpectedFieldsAndHnswVectorProfile()
    {
        var index = AzureAiSearchIndexInitializer.CreateIndex(
            "brewup-knowledge-test");

        Assert.Equal("brewup-knowledge-test", index.Name);
        Assert.Equal(12, index.Fields.Count);

        AssertField(index, "id", SearchFieldDataType.String, isKey: true);
        AssertField(index, "chunkId", SearchFieldDataType.String, isFilterable: true);
        AssertField(index, "documentId", SearchFieldDataType.String, isFilterable: true);
        AssertField(
            index,
            "sequence",
            SearchFieldDataType.Int32,
            isFilterable: true,
            isSortable: true);
        AssertField(
            index,
            "title",
            SearchFieldDataType.String,
            isSearchable: true,
            isFilterable: true);
        AssertField(index, "scope", SearchFieldDataType.String, isFilterable: true);
        AssertField(
            index,
            "tags",
            SearchFieldDataType.Collection(SearchFieldDataType.String),
            isFilterable: true);
        AssertField(
            index,
            "content",
            SearchFieldDataType.String,
            isSearchable: true);
        AssertField(index, "tokenCount", SearchFieldDataType.Int32, isFilterable: true);
        AssertField(index, "maxCharacters", SearchFieldDataType.Int32, isFilterable: true);
        AssertField(index, "overlapCharacters", SearchFieldDataType.Int32, isFilterable: true);

        var embedding = Assert.Single(index.Fields, field => field.Name == "embedding");
        Assert.Equal(
            SearchFieldDataType.Collection(SearchFieldDataType.Single),
            embedding.Type);
        Assert.Equal(1536, embedding.VectorSearchDimensions);
        Assert.Equal("knowledge-vector-profile", embedding.VectorSearchProfileName);

        Assert.NotNull(index.VectorSearch);
        Assert.IsType<HnswAlgorithmConfiguration>(
            Assert.Single(index.VectorSearch.Algorithms));
        var profile = Assert.Single(index.VectorSearch.Profiles);
        Assert.Equal("knowledge-vector-profile", profile.Name);
        Assert.Equal("knowledge-hnsw", profile.AlgorithmConfigurationName);
    }

    private static void AssertField(
        SearchIndex index,
        string name,
        SearchFieldDataType type,
        bool? isKey = null,
        bool? isSearchable = null,
        bool? isFilterable = null,
        bool? isSortable = null)
    {
        var field = Assert.Single(index.Fields, candidate => candidate.Name == name);
        Assert.Equal(type, field.Type);
        Assert.Equal(isKey, field.IsKey);
        Assert.Equal(isSearchable, field.IsSearchable);
        Assert.Equal(isFilterable, field.IsFilterable);
        Assert.Equal(isSortable, field.IsSortable);
    }
}
