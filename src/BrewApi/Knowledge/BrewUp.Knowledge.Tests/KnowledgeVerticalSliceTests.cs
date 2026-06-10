using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Core.Embeddings;
using BrewUp.Knowledge.Facade;
using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.Facade.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewUp.Knowledge.Tests;

public sealed class KnowledgeVerticalSliceTests
{
    [Fact]
    public async Task IngestAndSearch_RanksSemanticMatchAndAppliesScope()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var facade = scope.ServiceProvider.GetRequiredService<IKnowledgeFacade>();

        await facade.IngestAsync(new IngestKnowledgeDocumentRequest
        {
            Title = "IPA brewing",
            Content = "An IPA uses generous hop additions. Fermentation converts malt sugars into alcohol.",
            Source = DocumentSource.Markdown,
            Scope = DocumentScope.General
        }, CancellationToken.None);

        await facade.IngestAsync(new IngestKnowledgeDocumentRequest
        {
            Title = "Warehouse picking",
            Content = "Warehouse operators pick stock from bins and prepare pallets for shipping.",
            Scope = DocumentScope.Warehouse
        }, CancellationToken.None);

        var result = await facade.SearchAsync(new SearchKnowledgeBaseRequest
        {
            Query = "How are hops used when brewing beer?",
            MaxResults = 2
        }, CancellationToken.None);

        Assert.Equal("IPA brewing", result.Matches.First().Title);

        var warehouseResult = await facade.SearchAsync(new SearchKnowledgeBaseRequest
        {
            Query = "beer and hops",
            Scope = DocumentScope.Warehouse,
            MaxResults = 5
        }, CancellationToken.None);

        Assert.Single(warehouseResult.Matches);
        Assert.Equal("Warehouse picking", warehouseResult.Matches.Single().Title);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddKnowledge(configuration);
        services.RemoveAll<IEmbeddingGenerator>();
        services.AddSingleton<IEmbeddingGenerator, KeywordEmbeddingGenerator>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private sealed class KeywordEmbeddingGenerator : IEmbeddingGenerator
    {
        public Task<EmbeddingVector> GenerateAsync(string text, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalized = text.ToLowerInvariant();

            var vector = new[]
            {
                Count(normalized, "beer", "brew", "hop", "malt", "ferment", "ipa"),
                Count(normalized, "warehouse", "stock", "bin", "pallet", "shipping", "pick"),
                0.1f
            };

            return Task.FromResult(new EmbeddingVector(vector));
        }

        private static float Count(string text, params string[] terms)
            => terms.Count(text.Contains);
    }
}
