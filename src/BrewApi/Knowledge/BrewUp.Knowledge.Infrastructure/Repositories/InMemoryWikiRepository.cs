using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class InMemoryWikiRepository(
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeChunkRepository chunkRepository,
    WikiOptions options) : IWikiRepository
{
    private readonly Lock _lock = new();
    private readonly Dictionary<Guid, WikiProcessingJob> _jobs = [];
    private readonly Dictionary<Guid, WikiPage> _pages = [];
    private readonly List<WikiRevision> _revisions = [];
    private readonly List<WikiClaim> _claims = [];
    private readonly List<WikiEvidence> _evidence = [];
    private readonly List<WikiLink> _links = [];
    private readonly List<WikiIssue> _issues = [];
    private readonly Dictionary<Guid, EmbeddingVector> _embeddings = [];

    public Task<WikiProcessingStatus> EnqueueAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!options.Enabled)
            return Task.FromResult(WikiProcessingStatus.Disabled);

        lock (_lock)
        {
            var existing = _jobs.Values
                .Where(job => job.DocumentId == documentId)
                .OrderByDescending(job => job.CreatedAt)
                .FirstOrDefault();
            if (existing is not null &&
                existing.Status is WikiProcessingStatus.Pending or WikiProcessingStatus.Processing)
            {
                return Task.FromResult(existing.Status);
            }

            var now = DateTime.UtcNow;
            var job = new WikiProcessingJob(
                Guid.CreateVersion7(),
                documentId,
                WikiProcessingStatus.Pending,
                0,
                now,
                now,
                null,
                null);
            _jobs[job.Id] = job;
            return Task.FromResult(WikiProcessingStatus.Pending);
        }
    }

    public async Task<int> EnqueueMissingDocumentsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!options.Enabled)
            return 0;

        var documents = await documentRepository.GetAllAsync(cancellationToken);
        var enqueued = 0;
        lock (_lock)
        {
            foreach (var document in documents.Where(
                         document => _jobs.Values.All(job => job.DocumentId != document.Id)))
            {
                var now = DateTime.UtcNow;
                var job = new WikiProcessingJob(
                    Guid.CreateVersion7(),
                    document.Id,
                    WikiProcessingStatus.Pending,
                    0,
                    now,
                    now,
                    null,
                    null);
                _jobs[job.Id] = job;
                enqueued++;
            }
        }

        return enqueued;
    }

    public Task<WikiProcessingJob?> LeaseNextJobAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var leaseExpiredBefore = now.AddSeconds(-Math.Max(1, options.LeaseDurationSeconds));
            foreach (var exhausted in _jobs.Values
                         .Where(candidate =>
                             candidate.Status == WikiProcessingStatus.Processing &&
                             candidate.AttemptCount >= Math.Max(1, options.MaximumAttempts) &&
                             candidate.UpdatedAt <= leaseExpiredBefore)
                         .ToArray())
            {
                _jobs[exhausted.Id] = exhausted with
                {
                    Status = WikiProcessingStatus.Failed,
                    UpdatedAt = now,
                    ErrorType = "WorkerLeaseExpired"
                };
            }

            var job = _jobs.Values
                .Where(candidate =>
                    (candidate.Status == WikiProcessingStatus.Pending &&
                     (candidate.NextAttemptAt is null || candidate.NextAttemptAt <= now)) ||
                    (candidate.Status == WikiProcessingStatus.Processing &&
                     candidate.AttemptCount < Math.Max(1, options.MaximumAttempts) &&
                     candidate.UpdatedAt <= leaseExpiredBefore))
                .OrderBy(candidate => candidate.CreatedAt)
                .FirstOrDefault();
            if (job is null)
                return Task.FromResult<WikiProcessingJob?>(null);

            job = job with
            {
                Status = WikiProcessingStatus.Processing,
                AttemptCount = job.AttemptCount + 1,
                UpdatedAt = now
            };
            _jobs[job.Id] = job;
            return Task.FromResult<WikiProcessingJob?>(job);
        }
    }

    public Task<IReadOnlyCollection<WikiPageCandidate>> GetCandidatesAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            IReadOnlyCollection<WikiPageCandidate> result = _pages.Values
                .OrderByDescending(page => page.UpdatedAt)
                .Take(Math.Max(0, limit))
                .Select(page => new WikiPageCandidate(
                    page.Id,
                    page.NormalizedKey,
                    page.Title,
                    page.PageType,
                    page.Content,
                    GetCurrentClaims(page).Select(claim => claim.Content).ToArray()))
                .ToArray();
            return Task.FromResult(result);
        }
    }

    public Task ApplyAnalysisAsync(
        WikiProcessingJob job,
        WikiAnalysisResult analysis,
        IReadOnlyDictionary<string, EmbeddingVector> pageEmbeddings,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            if (!_jobs.TryGetValue(job.Id, out var persistedJob) ||
                persistedJob.Status != WikiProcessingStatus.Processing ||
                persistedJob.UpdatedAt != job.UpdatedAt)
            {
                return Task.CompletedTask;
            }

            var pageByKey = _pages.Values.ToDictionary(page => page.NormalizedKey, StringComparer.Ordinal);
            var revisionByPage = new Dictionary<Guid, WikiRevision>();
            var now = DateTime.UtcNow;

            foreach (var proposal in analysis.Pages)
            {
                var page = ResolvePage(proposal, pageByKey);
                var previousRevision = page is null || page.CurrentRevision == 0
                    ? null
                    : _revisions.Last(revision => revision.PageId == page.Id);
                page ??= new WikiPage(
                    Guid.CreateVersion7(),
                    proposal.Key,
                    proposal.Title,
                    proposal.PageType,
                    proposal.Scope,
                    0,
                    string.Empty,
                    now,
                    now);
                var revisionNumber = page.CurrentRevision + 1;
                var revision = new WikiRevision(
                    Guid.CreateVersion7(),
                    page.Id,
                    revisionNumber,
                    proposal.Content,
                    job.DocumentId,
                    job.Id,
                    now);
                page = page with
                {
                    Title = proposal.Title,
                    PageType = proposal.PageType,
                    Scope = proposal.Scope,
                    CurrentRevision = revisionNumber,
                    Content = proposal.Content,
                    UpdatedAt = now
                };
                _pages[page.Id] = page;
                pageByKey[page.NormalizedKey] = page;
                _revisions.Add(revision);
                revisionByPage[page.Id] = revision;

                var replacedClaimKeys = proposal.Claims
                    .Select(claim => claim.Key)
                    .ToHashSet(StringComparer.Ordinal);
                var sequence = 0;
                if (previousRevision is not null)
                {
                    foreach (var sourceClaim in _claims
                                 .Where(claim =>
                                     claim.RevisionId == previousRevision.Id &&
                                     !replacedClaimKeys.Contains(claim.NormalizedKey))
                                 .OrderBy(claim => claim.Sequence)
                                 .ToArray())
                    {
                        var claim = new WikiClaim(
                            Guid.CreateVersion7(),
                            revision.Id,
                            sourceClaim.NormalizedKey,
                            sourceClaim.Content,
                            sequence++);
                        _claims.Add(claim);
                        _evidence.AddRange(_evidence
                            .Where(item => item.ClaimId == sourceClaim.Id)
                            .ToArray()
                            .Select(item => item with
                            {
                                Id = Guid.CreateVersion7(),
                                RevisionId = revision.Id,
                                ClaimId = claim.Id
                            }));
                    }
                }

                foreach (var claimProposal in proposal.Claims)
                {
                    var claim = new WikiClaim(
                        Guid.CreateVersion7(),
                        revision.Id,
                        claimProposal.Key,
                        claimProposal.Content,
                        sequence++);
                    _claims.Add(claim);
                    _evidence.AddRange(claimProposal.EvidenceChunkIds.Select(chunkId =>
                        new WikiEvidence(
                            Guid.CreateVersion7(),
                            page.Id,
                            revision.Id,
                            claim.Id,
                            job.DocumentId,
                            chunkId,
                            WikiEvidenceStatus.Available,
                            now)));
                }

                if (pageEmbeddings.TryGetValue(proposal.Key, out var embedding))
                    _embeddings[page.Id] = embedding;
            }

            foreach (var linkProposal in analysis.Links)
            {
                var source = pageByKey[linkProposal.SourcePageKey];
                var target = pageByKey[linkProposal.TargetPageKey];
                var revision = revisionByPage.GetValueOrDefault(source.Id)
                               ?? _revisions.Last(revision => revision.PageId == source.Id);
                if (_links.Any(link =>
                        link.SourcePageId == source.Id &&
                        link.TargetPageId == target.Id &&
                        link.RelationshipType.Equals(
                            linkProposal.RelationshipType,
                            StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                _links.Add(new WikiLink(
                    Guid.CreateVersion7(),
                    source.Id,
                    target.Id,
                    linkProposal.RelationshipType,
                    revision.Id,
                    now));
            }

            foreach (var issueProposal in analysis.Issues)
            {
                var page = pageByKey[issueProposal.PageKey];
                var revision = revisionByPage.GetValueOrDefault(page.Id)
                               ?? _revisions.Last(revision => revision.PageId == page.Id);
                var claim = string.IsNullOrWhiteSpace(issueProposal.ClaimKey)
                    ? null
                    : _claims.LastOrDefault(candidate =>
                        candidate.RevisionId == revision.Id &&
                        candidate.NormalizedKey == issueProposal.ClaimKey);
                _issues.Add(new WikiIssue(
                    Guid.CreateVersion7(),
                    page.Id,
                    claim?.Id,
                    issueProposal.Type,
                    WikiIssueStatus.Open,
                    issueProposal.Description,
                    job.DocumentId,
                    now,
                    null));
            }

            _jobs[job.Id] = persistedJob with
            {
                Status = WikiProcessingStatus.Completed,
                UpdatedAt = now,
                NextAttemptAt = null,
                ErrorType = null
            };
        }

        return Task.CompletedTask;
    }

    public Task MarkJobFailedAsync(
        WikiProcessingJob job,
        string errorType,
        DateTime? nextAttemptAt,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            if (_jobs.TryGetValue(job.Id, out var persisted) &&
                persisted.Status == WikiProcessingStatus.Processing &&
                persisted.UpdatedAt == job.UpdatedAt)
            {
                _jobs[job.Id] = persisted with
                {
                    Status = nextAttemptAt is null
                        ? WikiProcessingStatus.Failed
                        : WikiProcessingStatus.Pending,
                    UpdatedAt = DateTime.UtcNow,
                    NextAttemptAt = nextAttemptAt,
                    ErrorType = errorType
                };
            }
        }

        return Task.CompletedTask;
    }

    public Task MarkEvidenceUnavailableAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            for (var index = 0; index < _evidence.Count; index++)
            {
                if (_evidence[index].DocumentId == documentId)
                    _evidence[index] = _evidence[index] with { Status = WikiEvidenceStatus.Unavailable };
            }

            var now = DateTime.UtcNow;
            foreach (var job in _jobs.Values
                         .Where(candidate =>
                             candidate.DocumentId == documentId &&
                             candidate.Status is WikiProcessingStatus.Pending or WikiProcessingStatus.Processing)
                         .ToArray())
            {
                _jobs[job.Id] = job with
                {
                    Status = WikiProcessingStatus.Failed,
                    UpdatedAt = now,
                    NextAttemptAt = null,
                    ErrorType = "SourceDocumentChanged"
                };
            }
        }

        return Task.CompletedTask;
    }

    public Task<WikiProcessingJob?> GetJobByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
            return Task.FromResult(_jobs.Values
                .Where(job => job.DocumentId == documentId)
                .OrderByDescending(job => job.CreatedAt)
                .FirstOrDefault());
    }

    public Task<WikiPageResult?> GetPageAsync(
        string normalizedKey,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var page = _pages.Values.SingleOrDefault(
                candidate => candidate.NormalizedKey == normalizedKey);
            if (page is null)
                return Task.FromResult<WikiPageResult?>(null);

            return Task.FromResult<WikiPageResult?>(new WikiPageResult(
                page,
                GetCurrentClaims(page),
                _links.Where(link => link.SourcePageId == page.Id).ToArray(),
                _issues.Where(issue => issue.PageId == page.Id).ToArray()));
        }
    }

    public async Task<WikiPageEvidenceResult?> GetPageEvidenceAsync(
        Guid pageId,
        CancellationToken cancellationToken)
    {
        WikiPage? page;
        WikiEvidence[] evidence;
        Dictionary<Guid, WikiClaim> claims;
        lock (_lock)
        {
            if (!_pages.TryGetValue(pageId, out page))
                return null;

            var revision = _revisions.Last(item => item.PageId == pageId);
            evidence = _evidence.Where(item => item.RevisionId == revision.Id).ToArray();
            claims = _claims.Where(item => item.RevisionId == revision.Id)
                .ToDictionary(item => item.Id);
        }

        var documents = (await documentRepository.GetAllAsync(cancellationToken))
            .ToDictionary(document => document.Id);
        var chunksByDocument = new Dictionary<Guid, IReadOnlyCollection<SharedKernel.Chunks.KnowledgeChunk>>();
        var items = new List<WikiPageEvidenceItem>(evidence.Length);

        foreach (var item in evidence)
        {
            documents.TryGetValue(item.DocumentId, out var document);
            if (!chunksByDocument.TryGetValue(item.DocumentId, out var chunks))
            {
                chunks = await chunkRepository.GetByDocumentIdAsync(
                    item.DocumentId,
                    cancellationToken);
                chunksByDocument[item.DocumentId] = chunks;
            }

            var chunk = chunks.SingleOrDefault(candidate => candidate.Id == item.ChunkId);
            items.Add(new WikiPageEvidenceItem(
                item,
                claims[item.ClaimId].Content,
                document?.Title,
                chunk?.Sequence,
                item.Status == WikiEvidenceStatus.Available ? chunk?.KnowledgeContent : null));
        }

        return new WikiPageEvidenceResult(page.Id, items);
    }

    public Task<IReadOnlyCollection<(WikiPage Page, double Score)>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int topK,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            IReadOnlyCollection<(WikiPage Page, double Score)> result = _pages.Values
                .Where(page => scope is null || page.Scope == scope)
                .Where(page => _embeddings.ContainsKey(page.Id))
                .Select(page => (page, Score(queryEmbedding, _embeddings[page.Id])))
                .OrderByDescending(item => item.Item2)
                .ThenBy(item => item.page.Title)
                .Take(topK)
                .ToArray();
            return Task.FromResult(result);
        }
    }

    private WikiPage? ResolvePage(
        WikiPageProposal proposal,
        IReadOnlyDictionary<string, WikiPage> pageByKey)
    {
        if (proposal.ExistingPageId is { } pageId && _pages.TryGetValue(pageId, out var byId))
            return byId;

        return pageByKey.GetValueOrDefault(proposal.Key);
    }

    private WikiClaim[] GetCurrentClaims(WikiPage page)
    {
        var revision = _revisions.LastOrDefault(item =>
            item.PageId == page.Id && item.RevisionNumber == page.CurrentRevision);
        return revision is null
            ? []
            : _claims.Where(claim => claim.RevisionId == revision.Id)
                .OrderBy(claim => claim.Sequence)
                .ToArray();
    }

    private static double Score(EmbeddingVector left, EmbeddingVector right)
    {
        if (left.Values.Count != right.Values.Count || left.Values.Count == 0)
            return 0;

        double dot = 0;
        double leftMagnitude = 0;
        double rightMagnitude = 0;
        for (var index = 0; index < left.Values.Count; index++)
        {
            dot += left.Values[index] * right.Values[index];
            leftMagnitude += left.Values[index] * left.Values[index];
            rightMagnitude += right.Values[index] * right.Values[index];
        }

        var denominator = Math.Sqrt(leftMagnitude) * Math.Sqrt(rightMagnitude);
        return denominator == 0 ? 0 : dot / denominator;
    }
}
