using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Tests;

public sealed class SqlServerKnowledgeVectorStoreTests
{
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
}
