using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Core.Embeddings;

namespace BrewUp.Knowledge.Facade.Ingestion;

public sealed class IngestKnowledgeDocumentHandler(
    IChunkingStrategy chunkingStrategy,
    IEmbeddingGenerator embeddingGenerator,
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeVectorStore vectorStore,
    IEnumerable<IKnowledgeTextExtractor> textExtractors)
{
    public async Task<IngestKnowledgeDocumentResult> HandleAsync(
        IngestKnowledgeDocumentCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("A document title is required.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.Content))
            throw new ArgumentException("Document content is required.", nameof(command));

        var document = new KnowledgeDocument
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            Content = command.Content,
            Scope = command.Scope,
            Source = command.Source,
            Tags = NormalizeTags(command.Tags),
            ImportedAt = DateTime.UtcNow
        };

        var chunks = chunkingStrategy.Split(document);
        await documentRepository.StoreAsync(document, cancellationToken);

        foreach (var chunk in chunks)
        {
            var embedding = await embeddingGenerator.GenerateAsync(chunk.Content, cancellationToken);
            await vectorStore.StoreAsync(chunk, embedding, cancellationToken);
        }

        return new IngestKnowledgeDocumentResult(document.Id, chunks.Count);
    }

    public async Task<IngestKnowledgeDocumentResult> HandleAsync(
        IngestKnowledgeFileCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Content);

        if (string.IsNullOrWhiteSpace(command.FileName))
            throw new ArgumentException("A file name is required.", nameof(command));

        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        var extractor = textExtractors.FirstOrDefault(candidate => candidate.CanExtract(extension))
                        ?? throw new UnsupportedKnowledgeFileTypeException(
                            string.IsNullOrEmpty(extension) ? "(none)" : extension);

        var content = await extractor.ExtractAsync(command.Content, cancellationToken);
        var source = extension switch
        {
            ".txt" => DocumentSource.PlainText,
            ".md" => DocumentSource.Markdown,
            _ => throw new UnsupportedKnowledgeFileTypeException(extension)
        };

        return await HandleAsync(
            new IngestKnowledgeDocumentCommand(
                Path.GetFileNameWithoutExtension(command.FileName),
                content,
                command.Scope,
                source,
                command.Tags),
            cancellationToken);
    }

    private static IReadOnlyCollection<string> NormalizeTags(IReadOnlyCollection<string>? tags)
        => tags?
               .Where(tag => !string.IsNullOrWhiteSpace(tag))
               .Select(tag => tag.Trim())
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .ToArray()
           ?? [];
}
