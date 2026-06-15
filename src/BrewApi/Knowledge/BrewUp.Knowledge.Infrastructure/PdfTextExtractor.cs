using BrewUp.Knowledge.Infrastructure.Ingestion;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Exceptions;

namespace BrewUp.Knowledge.Infrastructure;

public sealed class PdfTextExtractor : IKnowledgeTextExtractor
{
    private static readonly ContentOrderTextExtractor.Options ExtractionOptions = new()
    {
        SeparateParagraphsWithDoubleNewline = true,
        ReplaceWhitespaceWithSpace = true
    };

    public bool CanExtract(string fileExtension)
        => string.Equals(fileExtension, ".pdf", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);

        if (buffer.Length == 0)
            throw new InvalidDataException("The PDF is empty.");

        try
        {
            using var document = PdfDocument.Open(buffer.ToArray());

            if (document.NumberOfPages == 0)
                throw new InvalidDataException("The PDF is empty and contains no pages.");

            var pages = new List<string>(document.NumberOfPages);
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pageText = ContentOrderTextExtractor
                    .GetText(page, ExtractionOptions)
                    .Trim();

                if (!string.IsNullOrWhiteSpace(pageText))
                    pages.Add(pageText);
            }

            if (pages.Count == 0)
            {
                throw new InvalidDataException(
                    "The PDF contains no extractable text. " +
                    "Scanned PDFs are not supported yet.");
            }

            return string.Join(Environment.NewLine + Environment.NewLine, pages);
        }
        catch (PdfDocumentEncryptedException exception)
        {
            throw new InvalidDataException(
                "The PDF is encrypted or password-protected and cannot be read.",
                exception);
        }
        catch (PdfDocumentFormatException exception)
        {
            throw new InvalidDataException(
                "The PDF could not be read because its format is invalid or unsupported.",
                exception);
        }
    }
}
