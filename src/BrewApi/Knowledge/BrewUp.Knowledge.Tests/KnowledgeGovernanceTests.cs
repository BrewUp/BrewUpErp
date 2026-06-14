using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Facade;
using BrewUp.Knowledge.Facade.Evaluation;
using BrewUp.Knowledge.Facade.Governance;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Repositories;
using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.CustomTypes;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Tests;

public sealed class KnowledgeGovernanceTests
{
    [Fact]
    public async Task ListAndGetDocument_ReturnPersistedMetadataContentAndChunkCount()
    {
        await using var provider = CreateProvider();
        var ingested = await IngestAsync(provider);
        using var scope = provider.CreateScope();

        var list = await scope.ServiceProvider
            .GetRequiredService<GetKnowledgeDocumentsHandler>()
            .HandleAsync(CancellationToken.None);
        var detail = await scope.ServiceProvider
            .GetRequiredService<GetKnowledgeDocumentHandler>()
            .HandleAsync(ingested.DocumentId, CancellationToken.None);

        var summary = Assert.Single(list.Documents);
        Assert.Equal(ingested.DocumentId, summary.Id);
        Assert.Equal("IPA brewing guide", summary.Title);
        Assert.Equal("production", summary.Scope);
        Assert.Equal("plaintext", summary.Source);
        Assert.Equal(["ipa", "brewing"], summary.Tags);
        Assert.Equal(ingested.ChunkCount, summary.ChunkCount);

        Assert.NotNull(detail);
        Assert.Contains("dry hopping", detail.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ingested.ChunkCount, detail.ChunkCount);
    }

    [Fact]
    public async Task DeleteDocument_RemovesDocumentChunksAndVectors()
    {
        await using var provider = CreateProvider();
        var ingested = await IngestAsync(provider);
        using var scope = provider.CreateScope();

        var deleted = await scope.ServiceProvider
            .GetRequiredService<DeleteKnowledgeDocumentHandler>()
            .HandleAsync(ingested.DocumentId, CancellationToken.None);

        var document = await scope.ServiceProvider
            .GetRequiredService<IKnowledgeDocumentRepository>()
            .GetByIdAsync(ingested.DocumentId, CancellationToken.None);
        var chunks = await scope.ServiceProvider
            .GetRequiredService<IKnowledgeChunkRepository>()
            .GetByDocumentIdAsync(ingested.DocumentId, CancellationToken.None);
        var vectors = scope.ServiceProvider
            .GetRequiredService<InMemoryKnowledgeVectorStore>();

        Assert.True(deleted);
        Assert.Null(document);
        Assert.Empty(chunks);
        Assert.Equal(0, vectors.Count);
    }

    [Fact]
    public async Task ReindexDocument_ReplacesChunksAndEmbeddings()
    {
        await using var provider = CreateProvider();
        var ingested = await IngestAsync(provider);
        using var scope = provider.CreateScope();
        var chunks = scope.ServiceProvider.GetRequiredService<IKnowledgeChunkRepository>();
        var oldChunks = await chunks.GetByDocumentIdAsync(
            ingested.DocumentId,
            CancellationToken.None);

        var result = await scope.ServiceProvider
            .GetRequiredService<ReindexKnowledgeDocumentHandler>()
            .HandleAsync(ingested.DocumentId, CancellationToken.None);
        var newChunks = await chunks.GetByDocumentIdAsync(
            ingested.DocumentId,
            CancellationToken.None);
        var vectors = scope.ServiceProvider
            .GetRequiredService<InMemoryKnowledgeVectorStore>();

        Assert.NotNull(result);
        Assert.Equal(newChunks.Count, result.ChunkCount);
        Assert.Equal(newChunks.Count, vectors.Count);
        Assert.Empty(oldChunks.Select(chunk => chunk.Id)
            .Intersect(newChunks.Select(chunk => chunk.Id)));
    }

    [Fact]
    public async Task SearchAndEvaluation_IncludeAttributionAndDiagnostics()
    {
        await using var provider = CreateProvider();
        var ingested = await IngestAsync(provider);
        using var scope = provider.CreateScope();

        var search = await scope.ServiceProvider
            .GetRequiredService<SearchKnowledgeHandler>()
            .HandleAsync(
                new SearchKnowledgeQuery(
                    "How long should dry hopping last?",
                    "Production",
                    5),
                CancellationToken.None);
        var evaluation = await scope.ServiceProvider
            .GetRequiredService<KnowledgeRetrievalEvaluator>()
            .EvaluateAsync(
                new KnowledgeRetrievalTestCase(
                    "How long should dry hopping last?",
                    "IPA brewing guide",
                    "Production",
                    ["3 and 7 days"]),
                CancellationToken.None);

        var item = Assert.Single(search.Items);
        Assert.Equal(ingested.DocumentId, item.DocumentId);
        Assert.Equal("IPA brewing guide", item.DocumentTitle);
        Assert.Equal("production", item.Scope);
        Assert.Equal(0, item.ChunkSequence);
        Assert.NotEqual(Guid.Empty, item.ChunkId);
        Assert.NotEmpty(item.Tags);
        Assert.InRange(item.Score, -1d, 1d);

        Assert.True(evaluation.Passed);
        Assert.True(evaluation.ExpectedDocumentFound);
        Assert.Empty(evaluation.MissingContentTerms);
        Assert.Contains("Passed", evaluation.Diagnostic);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddKnowledgeFacade();
        services.AddSingleton<IEmbeddingGenerator, TestEmbeddingGenerator>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static async Task<IngestKnowledgeDocumentResult> IngestAsync(
        ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        return await scope.ServiceProvider
            .GetRequiredService<IngestKnowledgeDocumentHandler>()
            .HandleAsync(
                new IngestKnowledgeDocument(
                    "IPA brewing guide",
                    "Dry hopping contact time should remain between 3 and 7 days.",
                    DocumentScope.Production,
                    DocumentSource.PlainText,
                    ["ipa", "brewing"]),
                CancellationToken.None);
    }

    private sealed class TestEmbeddingGenerator : IEmbeddingGenerator
    {
        public Task<EmbeddingVector> GenerateAsync(
            string text,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalized = text.ToLowerInvariant();

            return Task.FromResult(new EmbeddingVector(
            [
                Score(normalized, "dry", "hopping", "contact", "days"),
                Score(normalized, "fermentation", "temperature")
            ]));
        }

        private static float Score(string text, params string[] terms)
            => terms.Count(text.Contains);
    }
}
