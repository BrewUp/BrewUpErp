using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.SharedKernel.Wiki;

public sealed record WikiAnalysisContext(
    KnowledgeDocument Document,
    IReadOnlyCollection<KnowledgeChunk> Chunks,
    IReadOnlyCollection<WikiPageCandidate> ExistingPages);

public sealed record WikiPageCandidate(
    Guid Id,
    string NormalizedKey,
    string Title,
    string PageType,
    string Content,
    IReadOnlyCollection<string> Claims);

public sealed record WikiAnalysisResult(
    IReadOnlyCollection<WikiPageProposal> Pages,
    IReadOnlyCollection<WikiLinkProposal> Links,
    IReadOnlyCollection<WikiIssueProposal> Issues)
{
    public static WikiAnalysisResult NoChange { get; } = new([], [], []);
}

public sealed record WikiPageProposal(
    Guid? ExistingPageId,
    string Key,
    string Title,
    string PageType,
    string Content,
    DocumentScope Scope,
    IReadOnlyCollection<WikiClaimProposal> Claims);

public sealed record WikiClaimProposal(
    string Key,
    string Content,
    IReadOnlyCollection<Guid> EvidenceChunkIds);

public sealed record WikiLinkProposal(
    string SourcePageKey,
    string TargetPageKey,
    string RelationshipType);

public sealed record WikiIssueProposal(
    string PageKey,
    string? ClaimKey,
    WikiIssueType Type,
    string Description);

