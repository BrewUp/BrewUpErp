using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.Tests;

public sealed class SemanticChunkingStrategyTests
{
    [Fact]
    public void Split_PreservesDocumentMetadataAndSequence()
    {
        var documentId = Guid.NewGuid();
        var document = new KnowledgeDocument
        {
            Id = documentId,
            Title = "Brewing guide",
            DocumentsContent = string.Join("\n\n", Enumerable.Repeat(
                "A paragraph about malt, hops, yeast, temperature, and fermentation.",
                8)),
            Source = DocumentSource.Markdown,
            Scope = DocumentScope.General,
            ImportedAt = DateTime.UtcNow
        };

        DefaultChunkingPolicy policy = new();
        var options = policy.GetOptionsFor(document);
        var chunks = new SemanticChunkingStrategy(policy)
            .Split(document)
            .ToArray();

        Assert.True(chunks.Length > 1);
        Assert.Equal(Enumerable.Range(0, chunks.Length), chunks.Select(chunk => chunk.Sequence));
        Assert.All(chunks, chunk =>
        {
            Assert.Equal(documentId, chunk.DocumentId);
            Assert.Equal("Brewing guide", chunk.Metadata.Title);
            Assert.Equal(DocumentScope.General, chunk.Metadata.Scope);
            Assert.True(chunk.Metadata.TokenCount > 0);
            Assert.True(chunk.KnowledgeContent.Length <= options.MaxCharacters);
        });
    }
}
