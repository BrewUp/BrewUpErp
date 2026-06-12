using System.Text;
using BrewUp.Knowledge.Infrastructure.Ingestion;

namespace BrewUp.Knowledge.Infrastructure;

public sealed class MarkdownTextExtractor : IKnowledgeTextExtractor
{
    public bool CanExtract(string fileExtension)
        => string.Equals(fileExtension, ".md", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        using var reader = new StreamReader(
            content,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}
