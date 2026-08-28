using System.Text.RegularExpressions;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.Core.Wiki;

public sealed class WikiAnalysisValidator(WikiOptions options)
{
    private static readonly Regex[] OperationalStatePatterns =
    [
        CreateOperationalStatePattern(
            @"\b(?:current stock|stock on hand)\s+(?:is|equals|stands at|totals?)\s+\d+(?:[.,]\d+)?\b"),
        CreateOperationalStatePattern(
            @"\b(?:there (?:is|are)\s+)?\d+\s+open sales orders\b"),
        CreateOperationalStatePattern(
            @"\bopen sales orders\s+(?:is|are|equal|total)\s+\d+\b"),
        CreateOperationalStatePattern(
            @"\bcurrently available\s*[:=]\s*\d+(?:[.,]\d+)?\b"),
        CreateOperationalStatePattern(
            @"\blive inventory\s*[:=]\s*\d+(?:[.,]\d+)?\b")
    ];

    public WikiAnalysisResult Validate(
        WikiAnalysisResult analysis,
        WikiAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(context);

        if (analysis.Pages.Count > options.MaximumPagesPerAnalysis)
            throw new InvalidOperationException(
                $"Wiki analysis proposed more than {options.MaximumPagesPerAnalysis} pages.");

        var validChunkIds = context.Chunks.Select(chunk => chunk.Id).ToHashSet();
        var existingPages = context.ExistingPages.ToDictionary(page => page.Id);
        var pages = new List<WikiPageProposal>(analysis.Pages.Count);
        var pageKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var proposal in analysis.Pages)
        {
            var normalizedKey = WikiKeyNormalizer.Normalize(proposal.Key);
            if (string.IsNullOrWhiteSpace(normalizedKey))
                throw new InvalidOperationException("Every Wiki page requires a valid key.");

            if (!pageKeys.Add(normalizedKey))
                throw new InvalidOperationException(
                    $"Wiki analysis proposed the duplicate page key '{normalizedKey}'.");

            if (proposal.ExistingPageId is { } existingPageId &&
                !existingPages.ContainsKey(existingPageId))
            {
                throw new InvalidOperationException(
                    $"Wiki analysis referenced unknown page '{existingPageId}'.");
            }

            if (string.IsNullOrWhiteSpace(proposal.Title) ||
                string.IsNullOrWhiteSpace(proposal.PageType) ||
                string.IsNullOrWhiteSpace(proposal.Content))
            {
                throw new InvalidOperationException(
                    $"Wiki page '{normalizedKey}' requires a title, type, and content.");
            }

            if (proposal.Content.Length > options.MaximumContentLength)
                throw new InvalidOperationException(
                    $"Wiki page '{normalizedKey}' exceeds the content limit.");

            if (ContainsOperationalState(proposal.Content))
                throw new InvalidOperationException(
                    $"Wiki page '{normalizedKey}' contains operational ERP state.");

            if (proposal.Claims.Count > options.MaximumClaimsPerPage)
                throw new InvalidOperationException(
                    $"Wiki page '{normalizedKey}' proposed too many claims.");

            var claimKeys = new HashSet<string>(StringComparer.Ordinal);
            var claims = proposal.Claims.Select(claim =>
            {
                var claimKey = WikiKeyNormalizer.Normalize(claim.Key);
                if (string.IsNullOrWhiteSpace(claimKey) || !claimKeys.Add(claimKey))
                    throw new InvalidOperationException(
                        $"Wiki page '{normalizedKey}' contains an invalid or duplicate claim key.");

                if (string.IsNullOrWhiteSpace(claim.Content))
                    throw new InvalidOperationException(
                        $"Wiki claim '{claimKey}' requires content.");

                if (ContainsOperationalState(claim.Content))
                    throw new InvalidOperationException(
                        $"Wiki claim '{claimKey}' contains operational ERP state.");

                if (claim.EvidenceChunkIds.Count == 0 ||
                    claim.EvidenceChunkIds.Any(chunkId => !validChunkIds.Contains(chunkId)))
                {
                    throw new InvalidOperationException(
                        $"Wiki claim '{claimKey}' must reference only chunks from the source document.");
                }

                return claim with
                {
                    Key = claimKey,
                    Content = claim.Content.Trim(),
                    EvidenceChunkIds = claim.EvidenceChunkIds.Distinct().ToArray()
                };
            }).ToArray();

            pages.Add(proposal with
            {
                Key = normalizedKey,
                Title = proposal.Title.Trim(),
                PageType = proposal.PageType.Trim(),
                Content = proposal.Content.Trim(),
                Claims = claims
            });
        }

        var availableKeys = pageKeys
            .Concat(context.ExistingPages.Select(page => page.NormalizedKey))
            .ToHashSet(StringComparer.Ordinal);

        var links = analysis.Links.Select(link =>
        {
            var sourceKey = WikiKeyNormalizer.Normalize(link.SourcePageKey);
            var targetKey = WikiKeyNormalizer.Normalize(link.TargetPageKey);
            if (!availableKeys.Contains(sourceKey) || !availableKeys.Contains(targetKey))
                throw new InvalidOperationException(
                    $"Wiki link '{sourceKey}' to '{targetKey}' references an unknown page.");

            if (sourceKey == targetKey || string.IsNullOrWhiteSpace(link.RelationshipType))
                throw new InvalidOperationException("Wiki links require distinct pages and a relationship type.");

            return link with
            {
                SourcePageKey = sourceKey,
                TargetPageKey = targetKey,
                RelationshipType = link.RelationshipType.Trim()
            };
        }).Distinct().ToArray();

        var issues = analysis.Issues.Select(issue =>
        {
            var pageKey = WikiKeyNormalizer.Normalize(issue.PageKey);
            if (!availableKeys.Contains(pageKey))
                throw new InvalidOperationException(
                    $"Wiki issue references unknown page '{pageKey}'.");

            return issue with
            {
                PageKey = pageKey,
                ClaimKey = string.IsNullOrWhiteSpace(issue.ClaimKey)
                    ? null
                    : WikiKeyNormalizer.Normalize(issue.ClaimKey),
                Description = issue.Description.Trim()
            };
        }).ToArray();

        return new WikiAnalysisResult(pages, links, issues);
    }

    private static bool ContainsOperationalState(string content)
        => OperationalStatePatterns.Any(pattern => pattern.IsMatch(content));

    private static Regex CreateOperationalStatePattern(string pattern)
        => new(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}
