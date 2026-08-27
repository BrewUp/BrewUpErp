namespace BrewUp.Knowledge.SharedKernel.Wiki;

public sealed record QueryWikiRequest(
    string Query,
    string? Scope = null,
    int? TopK = null);

public sealed record WikiSearchResult(IReadOnlyCollection<WikiSearchResultItem> Items);

public sealed record WikiSearchResultItem(
    Guid PageId,
    string Key,
    string Title,
    string PageType,
    string Scope,
    int Revision,
    string Content,
    double Score);

public sealed record WikiPageResult(
    WikiPage Page,
    IReadOnlyCollection<WikiClaim> Claims,
    IReadOnlyCollection<WikiLink> Links,
    IReadOnlyCollection<WikiIssue> Issues);

public sealed record WikiPageEvidenceItem(
    WikiEvidence Evidence,
    string Claim,
    string? DocumentTitle,
    int? ChunkSequence,
    string? SourceContent);

public sealed record WikiPageEvidenceResult(
    Guid PageId,
    IReadOnlyCollection<WikiPageEvidenceItem> Evidence);

public sealed record WikiProcessingJobResult(
    Guid DocumentId,
    WikiProcessingStatus Status,
    int AttemptCount,
    string? ErrorType,
    DateTime UpdatedAt);

