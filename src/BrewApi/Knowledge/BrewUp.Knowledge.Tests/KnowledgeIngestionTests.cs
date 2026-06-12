using System.Text;
using BrewUp.Knowledge.Core;
using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Exceptions;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Tests;

public sealed class KnowledgeIngestionTests
{
    [Fact]
    public async Task IngestPlainText_StoresDocumentChunksAndEmbeddings()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IngestKnowledgeDocumentHandler>();
        var documents = scope.ServiceProvider.GetRequiredService<InMemoryKnowledgeDocumentRepository>();
        var chunks = scope.ServiceProvider.GetRequiredService<InMemoryKnowledgeChunkRepository>();
        var vectors = scope.ServiceProvider.GetRequiredService<InMemoryKnowledgeVectorStore>();
        await using var content = StreamOf("Malt and hops are ingredients used to brew beer.");

        var result = await handler.HandleAsync(
            new IngestKnowledgeFile(
                "brewing.txt",
                content,
                DocumentScope.General,
                ["brewing", "guide"]),
            CancellationToken.None);

        Assert.Equal(1, result.ChunkCount);
        Assert.Equal(1, vectors.Count);
        Assert.Single(await chunks.GetByDocumentIdAsync(result.DocumentId, CancellationToken.None));
        Assert.True(documents.TryGet(result.DocumentId, out var document));
        Assert.Equal("brewing", document!.Title);
        Assert.Equal(DocumentSource.PlainText, document.Source);
        Assert.Equal(new[] { "brewing", "guide" }, document.Tags);
    }

    [Fact]
    public async Task IngestMarkdown_StoresExtractedMarkdownDocument()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IngestKnowledgeDocumentHandler>();
        var documents = scope.ServiceProvider.GetRequiredService<InMemoryKnowledgeDocumentRepository>();
        var vectors = scope.ServiceProvider.GetRequiredService<InMemoryKnowledgeVectorStore>();
        await using var content = StreamOf("# Brewing\n\nUse **hops** to add bitterness.");

        var result = await handler.HandleAsync(
            new IngestKnowledgeFile(
                "brewing.md",
                content,
                DocumentScope.General),
            CancellationToken.None);

        Assert.Equal(1, result.ChunkCount);
        Assert.Equal(1, vectors.Count);
        Assert.True(documents.TryGet(result.DocumentId, out var document));
        Assert.Equal(DocumentSource.Markdown, document!.Source);
        Assert.Contains("**hops**", document.Content);
    }

    [Fact]
    public async Task IngestFile_WithUnsupportedExtension_FailsExplicitly()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IngestKnowledgeDocumentHandler>();
        await using var content = StreamOf("PDF content is not supported yet.");

        var exception = await Assert.ThrowsAsync<UnsupportedKnowledgeFileTypeException>(
            () => handler.HandleAsync(
                new IngestKnowledgeFile(
                    "brewing.pdf",
                    content,
                    DocumentScope.General),
                CancellationToken.None));

        Assert.Contains(".pdf", exception.Message);
        Assert.Contains(".txt and .md", exception.Message);
    }

    [Fact]
    public async Task IngestDocument_ReturnsGeneratedChunkCount()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IngestKnowledgeDocumentHandler>();
        var vectors = scope.ServiceProvider.GetRequiredService<InMemoryKnowledgeVectorStore>();
        var chunks = scope.ServiceProvider.GetRequiredService<InMemoryKnowledgeChunkRepository>();
        var paragraphs = Enumerable.Repeat(
            "This paragraph documents brewing, fermentation, packaging, and warehouse procedures.",
            30);

        var result = await handler.HandleAsync(
            new IngestKnowledgeDocument(
                "Operations handbook",
                string.Join("\n\n", paragraphs),
                DocumentScope.General,
                DocumentSource.PlainText),
            CancellationToken.None);

        Assert.True(result.ChunkCount > 1);
        Assert.Equal(result.ChunkCount, vectors.Count);
        Assert.Equal(
            result.ChunkCount,
            (await chunks.GetByDocumentIdAsync(result.DocumentId, CancellationToken.None)).Count);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddCore();
        services.AddInfrastructure();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static MemoryStream StreamOf(string content)
        => new(Encoding.UTF8.GetBytes(content));
}
