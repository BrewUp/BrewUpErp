using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Repositories;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Tests;

public sealed class SqlServerKnowledgeVectorStoreTests
{
    [Fact]
    public void AddInfrastructure_BindsDedicatedEmbeddingConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BrewUp:SqlServer:ConnectionString"] =
                    "Server=localhost;Database=BrewUp;Integrated Security=true",
                ["BrewUp:Embeddings:Endpoint"] =
                    "https://brewup.cognitiveservices.azure.com",
                ["BrewUp:Embeddings:DeploymentName"] =
                    "text-embedding-3-small",
                ["BrewUp:Embeddings:Dimensions"] = "1536",
                ["BrewUp:Embeddings:UseManagedIdentity"] = "false",
                ["BrewUp:Embeddings:ApiKey"] = "test-key"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var options =
            provider.GetRequiredService<AzureOpenAiEmbeddingOptions>();

        Assert.Equal("text-embedding-3-small", options.DeploymentName);
        Assert.Equal(1536, options.Dimensions);
    }

    [Fact]
    public void VectorStoreOptions_DefaultToEmbeddingDimensions()
    {
        Assert.Equal(
            1536,
            new SqlServerKnowledgeVectorStoreOptions().Dimensions);
        Assert.Equal(1536, new AzureAiSearchOptions().Dimensions);
        Assert.Equal(1536, new AzureOpenAiEmbeddingOptions().Dimensions);
    }

    [Fact]
    public void AddInfrastructure_WithConfiguration_RegistersSqlServerVectorStore()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BrewUp:SqlServer:ConnectionString"] =
                    "Server=localhost;Database=BrewUp;Integrated Security=true",
                ["BrewUp:Knowledge:VectorStore:Dimensions"] = "256"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<SqlServerKnowledgeDocumentRepository>(
            provider.GetRequiredService<IKnowledgeDocumentRepository>());
        Assert.IsType<SqlServerKnowledgeChunkRepository>(
            provider.GetRequiredService<IKnowledgeChunkRepository>());
        Assert.IsType<SqlServerKnowledgeChunkRepository>(
            provider.GetRequiredService<IKnowledgeChunkWriter>());
        Assert.IsType<SqlServerKnowledgeVectorStore>(
            provider.GetRequiredService<IKnowledgeVectorStore>());
    }

    [Fact]
    public void AddInfrastructure_WithAzureAiSearchConfiguration_RegistersProjectionStore()
    {
        var configuration = CreateAzureAiSearchConfiguration();
        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<SqlServerKnowledgeDocumentRepository>(
            provider.GetRequiredService<IKnowledgeDocumentRepository>());
        Assert.IsType<SqlServerKnowledgeChunkRepository>(
            provider.GetRequiredService<IKnowledgeChunkRepository>());
        Assert.IsType<AzureAiSearchKnowledgeVectorStore>(
            provider.GetRequiredService<IKnowledgeVectorStore>());
        Assert.NotNull(provider.GetRequiredService<SqlServerKnowledgeVectorStore>());
        Assert.Equal(
            "brewup-knowledge-test",
            provider.GetRequiredService<AzureAiSearchOptions>().IndexName);
    }

    [Fact]
    public void AddInfrastructureForMcp_WithAzureAiSearchConfiguration_RegistersProjectionStore()
    {
        var configuration = CreateAzureAiSearchConfiguration();
        var services = new ServiceCollection();

        services.AddInfrastructureForMcp(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<AzureAiSearchKnowledgeVectorStore>(
            provider.GetRequiredService<IKnowledgeVectorStore>());
        Assert.IsType<SqlServerKnowledgeChunkRepository>(
            provider.GetRequiredService<IKnowledgeChunkRepository>());
    }

    private static IConfiguration CreateAzureAiSearchConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Knowledge:VectorStore"] = "AzureAiSearch",
                ["Knowledge:AzureAiSearch:Endpoint"] =
                    "https://brewup.search.windows.net",
                ["Knowledge:AzureAiSearch:IndexName"] = "brewup-knowledge-test",
                ["Knowledge:AzureAiSearch:ApiKey"] = "test-key",
                ["Knowledge:AzureAiSearch:UseManagedIdentity"] = "false",
                ["BrewUp:SqlServer:ConnectionString"] =
                    "Server=localhost;Database=BrewUp;Integrated Security=true"
            })
            .Build();
}
