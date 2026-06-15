using BrewUp.Knowledge.Infrastructure;

namespace BrewUp.Knowledge.Tests;

public sealed class PdfTextExtractorTests
{
    [Theory]
    [InlineData(".pdf")]
    [InlineData(".PDF")]
    public void CanExtract_PdfExtension_ReturnsTrue(string extension)
    {
        var extractor = new PdfTextExtractor();

        Assert.True(extractor.CanExtract(extension));
    }

    [Fact]
    public async Task ExtractAsync_EmptyPdf_FailsWithClearError()
    {
        var extractor = new PdfTextExtractor();
        await using var content = new MemoryStream();

        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => extractor.ExtractAsync(content, CancellationToken.None));

        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExtractAsync_PdfWithoutText_ExplainsThatScannedPdfsAreUnsupported()
    {
        var extractor = new PdfTextExtractor();
        await using var content = PdfTestDocument.Create(null);

        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => extractor.ExtractAsync(content, CancellationToken.None));

        Assert.Contains("no extractable text", exception.Message);
        Assert.Contains("Scanned PDFs are not supported", exception.Message);
    }
}
