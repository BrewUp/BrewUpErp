using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.SharedKernel.CustomTypes;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Exceptions;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;

namespace BrewUp.Knowledge.Core.CommandHandlers;

public sealed class IngestKnowledgeDocumentHandler(
    IChunkingStrategy chunkingStrategy,
    IEmbeddingGenerator embeddingGenerator,
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeChunkWriter chunkWriter,
    IKnowledgeVectorStore vectorStore,
    IEnumerable<IKnowledgeTextExtractor> textExtractors)
{
    public async Task<IngestKnowledgeDocumentResult> HandleAsync(
        IngestKnowledgeDocument command,
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
            DocumentsContent = command.Content,
            Scope = command.Scope,
            Source = command.Source,
            Tags = NormalizeTags(command.Tags),
            ImportedAt = DateTime.UtcNow
        };

        var chunks = chunkingStrategy.Split(document);
        await documentRepository.StoreAsync(document, cancellationToken);
        await chunkWriter.StoreAsync(chunks, cancellationToken);

        foreach (var chunk in chunks)
        {
            var embedding = await embeddingGenerator.GenerateAsync(chunk.KnowledgeContent, cancellationToken);
            await vectorStore.StoreAsync(chunk, embedding, cancellationToken);
        }

        return new IngestKnowledgeDocumentResult(document.Id, chunks.Count);
    }

    public async Task<IngestKnowledgeDocumentResult> HandleAsync(
        IngestKnowledgeFile command,
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
            ".pdf" => DocumentSource.Pdf,
            _ => throw new UnsupportedKnowledgeFileTypeException(extension)
        };

        return await HandleAsync(
            new IngestKnowledgeDocument(
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
