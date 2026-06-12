using BrewUp.Knowledge.Core;
using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Tests;

public sealed class SearchKnowledgeTests
{
    [Theory]
    [InlineData(
        "How long should dry hopping last?",
        "Contact time between 3 and 7 days")]
    [InlineData(
        "What temperature should fermentation use?",
        "Fermentation temperature should remain between 18 and 21 C")]
    [InlineData(
        "What causes weak hop aroma?",
        "Common brewing problems include weak hop aroma")]
    public async Task Search_ReturnsRelevantChunksAfterIngestion(
        string query,
        string expectedContent)
    {
        await using var provider = CreateProvider();
        await IngestGuides(provider);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<SearchKnowledgeHandler>();

        var result = await handler.HandleAsync(
            new SearchKnowledgeQuery(query, "Production", 3),
            CancellationToken.None);

        Assert.NotEmpty(result.Items);
        Assert.Contains(expectedContent, result.Items.First().Content);
        Assert.All(result.Items, item => Assert.InRange(item.Score, -1d, 1d));
    }

    [Fact]
    public async Task Search_TopKLimitsResultsAndCapsAtMaximum()
    {
        await using var provider = CreateProvider();
        await IngestDocuments(provider, 25, DocumentScope.Production);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<SearchKnowledgeHandler>();

        var limited = await handler.HandleAsync(
            new SearchKnowledgeQuery("dry hopping duration", TopK: 2),
            CancellationToken.None);
        var defaulted = await handler.HandleAsync(
            new SearchKnowledgeQuery("dry hopping duration"),
            CancellationToken.None);
        var capped = await handler.HandleAsync(
            new SearchKnowledgeQuery("dry hopping duration", TopK: 100),
            CancellationToken.None);

        Assert.Equal(2, limited.Items.Count);
        Assert.Equal(SearchKnowledgeHandler.DefaultTopK, defaulted.Items.Count);
        Assert.Equal(SearchKnowledgeHandler.MaximumTopK, capped.Items.Count);
    }

    [Fact]
    public async Task Search_FiltersByScopeBeforeLimiting()
    {
        await using var provider = CreateProvider();
        await IngestDocuments(provider, 3, DocumentScope.General);
        await IngestDocuments(provider, 2, DocumentScope.Production);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<SearchKnowledgeHandler>();

        var result = await handler.HandleAsync(
            new SearchKnowledgeQuery("dry hopping duration", "Production", 5),
            CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal("production", item.Scope));
    }

    [Fact]
    public async Task Search_OrdersResultsByScoreDescending()
    {
        await using var provider = CreateProvider();
        await IngestGuides(provider);
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<SearchKnowledgeHandler>();

        var result = await handler.HandleAsync(
            new SearchKnowledgeQuery("dry hopping duration", TopK: 3),
            CancellationToken.None);

        Assert.Equal(
            result.Items.OrderByDescending(item => item.Score).Select(item => item.Score),
            result.Items.Select(item => item.Score));
    }

    [Fact]
    public async Task Search_EmptyQueryReturnsValidationError()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<SearchKnowledgeHandler>();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(
                new SearchKnowledgeQuery(" "),
                CancellationToken.None));

        Assert.Equal("Query", exception.ParamName);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddCore();
        services.AddInfrastructure();
        services.AddScoped<SearchKnowledgeHandler>();
        services.AddSingleton<IEmbeddingGenerator, TestEmbeddingGenerator>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static async Task IngestGuides(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IngestKnowledgeDocumentHandler>();

        await Ingest(
            handler,
            "IPA dry hopping",
            "Dry hopping duration and contact time. Contact time between 3 and 7 days produces the best hop character.",
            DocumentScope.Production);
        await Ingest(
            handler,
            "IPA fermentation",
            "Fermentation temperature should remain between 18 and 21 C for a clean IPA fermentation.",
            DocumentScope.Production);
        await Ingest(
            handler,
            "Common brewing problems",
            "Common brewing problems include weak hop aroma caused by old hops or excessive oxygen exposure.",
            DocumentScope.Production);
    }

    private static async Task IngestDocuments(
        ServiceProvider provider,
        int count,
        DocumentScope documentScope)
    {
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IngestKnowledgeDocumentHandler>();

        for (var index = 0; index < count; index++)
        {
            await Ingest(
                handler,
                $"Dry hopping guide {index}",
                $"Dry hopping duration guide {index}. Contact time is carefully controlled.",
                documentScope);
        }
    }

    private static Task Ingest(
        IngestKnowledgeDocumentHandler handler,
        string title,
        string content,
        DocumentScope scope)
        => handler.HandleAsync(
            new IngestKnowledgeDocument(
                title,
                content,
                scope,
                DocumentSource.PlainText),
            CancellationToken.None);

    private sealed class TestEmbeddingGenerator : IEmbeddingGenerator
    {
        public Task<EmbeddingVector> GenerateAsync(
            string text,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalized = text.ToLowerInvariant();

            var values = new[]
            {
                Score(normalized, "dry", "hopping", "duration", "contact"),
                Score(normalized, "fermentation", "temperature"),
                Score(normalized, "weak", "hop", "aroma", "problems")
            };

            return Task.FromResult(new EmbeddingVector(values));
        }

        private static float Score(string text, params string[] terms)
            => terms.Count(text.Contains);
    }
}
