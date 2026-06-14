using System.Text;
using System.Text.RegularExpressions;
using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.Core.Chunking;

public sealed class SemanticChunkingStrategy(IChunkingPolicy policy) : IChunkingStrategy
{
    private static readonly Regex ParagraphSeparator = new(@"\r?\n\s*\r?\n", RegexOptions.Compiled);
    private int _maxCharacters;
    private int _overlapCharacters;

    public IReadOnlyCollection<KnowledgeChunk> Split(KnowledgeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        
        var options = policy.GetOptionsFor(document);
        _maxCharacters = options.MaxCharacters;
        _overlapCharacters = options.OverlapCharacters;

        if (string.IsNullOrWhiteSpace(document.DocumentsContent))
            return [];

        var normalizedContent = document.DocumentsContent.Replace("\r\n", "\n").Trim();
        var sections = ParagraphSeparator
            .Split(normalizedContent)
            .Select(section => section.Trim())
            .Where(section => section.Length > 0)
            .ToArray();

        var chunkContents = new List<string>();
        var current = new StringBuilder();

        foreach (var section in sections)
        {
            foreach (var part in SplitOversizedSection(section))
            {
                if (current.Length > 0 && current.Length + 2 + part.Length > _maxCharacters)
                {
                    AddChunk(chunkContents, current.ToString());
                    var overlap = GetOverlap(current.ToString());
                    current.Clear();

                    if (overlap.Length + 2 + part.Length <= _maxCharacters)
                        current.Append(overlap);
                }

                if (current.Length > 0)
                    current.AppendLine().AppendLine();

                current.Append(part);
            }
        }

        AddChunk(chunkContents, current.ToString());

        return chunkContents
            .Select((content, sequence) => new KnowledgeChunk
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                KnowledgeContent = content,
                Sequence = sequence,
                Metadata = new ChunkMetadata
                {
                    Scope = document.Scope,
                    Title = document.Title,
                    Tags = document.Tags,
                    TokenCount = EstimateTokenCount(content),
                    MaxCharacters = _maxCharacters,
                    OverlapCharacters = _overlapCharacters
                }
            })
            .ToArray();
    }

    private IEnumerable<string> SplitOversizedSection(string section)
    {
        var remaining = section;

        while (remaining.Length > _maxCharacters)
        {
            var splitAt = remaining.LastIndexOf(' ', _maxCharacters);
            if (splitAt < _maxCharacters / 2)
                splitAt = _maxCharacters;

            yield return remaining[..splitAt].Trim();
            remaining = remaining[splitAt..].TrimStart();
        }

        if (remaining.Length > 0)
            yield return remaining;
    }

    private string GetOverlap(string content)
    {
        if (_overlapCharacters == 0 || content.Length <= _overlapCharacters)
            return string.Empty;

        var overlapStart = content.Length - _overlapCharacters;
        var nextSpace = content.IndexOf(' ', overlapStart);
        return content[(nextSpace >= 0 ? nextSpace + 1 : overlapStart)..].Trim();
    }

    private static void AddChunk(ICollection<string> chunks, string content)
    {
        var trimmed = content.Trim();
        if (trimmed.Length > 0)
            chunks.Add(trimmed);
    }

    private static int EstimateTokenCount(string content)
        => Math.Max(1, (int)Math.Ceiling(content.Length / 4d));
}
