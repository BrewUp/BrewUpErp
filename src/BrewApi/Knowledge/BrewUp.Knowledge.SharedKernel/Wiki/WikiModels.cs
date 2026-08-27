using BrewUp.Knowledge.SharedKernel.Enums;

namespace BrewUp.Knowledge.SharedKernel.Wiki;

public enum WikiProcessingStatus
{
    Disabled,
    Pending,
    Processing,
    Completed,
    Failed
}

public enum WikiEvidenceStatus
{
    Available,
    Unavailable
}

public enum WikiIssueType
{
    ContradictoryEvidence,
    UnsupportedClaim,
    MissingEvidence,
    BrokenLink
}

public enum WikiIssueStatus
{
    Open,
    Resolved
}

public sealed record WikiPage(
    Guid Id,
    string NormalizedKey,
    string Title,
    string PageType,
    DocumentScope Scope,
    int CurrentRevision,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record WikiRevision(
    Guid Id,
    Guid PageId,
    int RevisionNumber,
    string Content,
    Guid SourceDocumentId,
    Guid ProcessingJobId,
    DateTime CreatedAt);

public sealed record WikiClaim(
    Guid Id,
    Guid RevisionId,
    string NormalizedKey,
    string Content,
    int Sequence);

public sealed record WikiEvidence(
    Guid Id,
    Guid PageId,
    Guid RevisionId,
    Guid ClaimId,
    Guid DocumentId,
    Guid ChunkId,
    WikiEvidenceStatus Status,
    DateTime AttachedAt);

public sealed record WikiLink(
    Guid Id,
    Guid SourcePageId,
    Guid TargetPageId,
    string RelationshipType,
    Guid RevisionId,
    DateTime CreatedAt);

public sealed record WikiIssue(
    Guid Id,
    Guid PageId,
    Guid? ClaimId,
    WikiIssueType Type,
    WikiIssueStatus Status,
    string Description,
    Guid SourceDocumentId,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

public sealed record WikiProcessingJob(
    Guid Id,
    Guid DocumentId,
    WikiProcessingStatus Status,
    int AttemptCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? NextAttemptAt,
    string? ErrorType);

