using System.Data;
using System.Text.Json;
using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Wiki;
using Microsoft.Data.SqlClient;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class SqlServerWikiRepository(
    SqlServerKnowledgeVectorStoreOptions options,
    WikiOptions wikiOptions) : IWikiRepository
{
    public async Task<WikiProcessingStatus> EnqueueAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (!wikiOptions.Enabled)
            return WikiProcessingStatus.Disabled;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        var now = DateTime.UtcNow;

        const string commandText = """
            SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
            BEGIN TRANSACTION;

            IF EXISTS
            (
                SELECT 1
                FROM [dbo].[KnowledgeWikiProcessingJobs] WITH (UPDLOCK, HOLDLOCK)
                WHERE DocumentId = @documentId
                  AND Status IN ('Pending', 'Processing')
            )
            BEGIN
                SELECT TOP (1) Status
                FROM [dbo].[KnowledgeWikiProcessingJobs]
                WHERE DocumentId = @documentId
                  AND Status IN ('Pending', 'Processing')
                ORDER BY CreatedAt DESC, Id DESC;
            END
            ELSE
            BEGIN
                INSERT INTO [dbo].[KnowledgeWikiProcessingJobs]
                    (Id, DocumentId, Status, AttemptCount, CreatedAt, UpdatedAt)
                VALUES
                    (@id, @documentId, 'Pending', 0, @now, @now);

                SELECT 'Pending';
            END;

            COMMIT TRANSACTION;
            """;

        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = Guid.CreateVersion7();
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = documentId;
        command.Parameters.Add("@now", SqlDbType.DateTime2).Value = now;
        var status = (string)(await command.ExecuteScalarAsync(cancellationToken)
                              ?? throw new InvalidOperationException("Wiki job could not be enqueued."));
        return Enum.Parse<WikiProcessingStatus>(status);
    }

    public async Task<int> EnqueueMissingDocumentsAsync(CancellationToken cancellationToken)
    {
        if (!wikiOptions.Enabled)
            return 0;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        const string commandText = """
            SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
            BEGIN TRANSACTION;

            INSERT INTO [dbo].[KnowledgeWikiProcessingJobs]
                (Id, DocumentId, Status, AttemptCount, CreatedAt, UpdatedAt)
            SELECT NEWID(), document.Id, 'Pending', 0, SYSUTCDATETIME(), SYSUTCDATETIME()
            FROM [dbo].[KnowledgeDocuments] document
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM [dbo].[KnowledgeWikiProcessingJobs] job WITH (UPDLOCK, HOLDLOCK)
                WHERE job.DocumentId = document.Id
            );

            DECLARE @inserted INT = @@ROWCOUNT;
            COMMIT TRANSACTION;
            SELECT @inserted;
            """;
        await using var command = new SqlCommand(commandText, connection);
        return (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
    }

    public async Task<WikiProcessingJob?> LeaseNextJobAsync(
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        const string commandText = """
            UPDATE [dbo].[KnowledgeWikiProcessingJobs]
            SET Status = 'Failed',
                UpdatedAt = SYSUTCDATETIME(),
                NextAttemptAt = NULL,
                ErrorType = 'WorkerLeaseExpired'
            WHERE Status = 'Processing'
              AND AttemptCount >= @maximumAttempts
              AND UpdatedAt <= DATEADD(SECOND, -@leaseDurationSeconds, SYSUTCDATETIME());

            ;WITH NextJob AS
            (
                SELECT TOP (1) *
                FROM [dbo].[KnowledgeWikiProcessingJobs] WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE
                    (Status = 'Pending'
                     AND (NextAttemptAt IS NULL OR NextAttemptAt <= SYSUTCDATETIME()))
                    OR
                    (Status = 'Processing'
                     AND AttemptCount < @maximumAttempts
                     AND UpdatedAt <= DATEADD(SECOND, -@leaseDurationSeconds, SYSUTCDATETIME()))
                ORDER BY CreatedAt, Id
            )
            UPDATE NextJob
            SET Status = 'Processing',
                AttemptCount = AttemptCount + 1,
                UpdatedAt = SYSUTCDATETIME()
            OUTPUT
                inserted.Id,
                inserted.DocumentId,
                inserted.Status,
                inserted.AttemptCount,
                inserted.CreatedAt,
                inserted.UpdatedAt,
                inserted.NextAttemptAt,
                inserted.ErrorType;
            """;

        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@maximumAttempts", SqlDbType.Int).Value =
            Math.Max(1, wikiOptions.MaximumAttempts);
        command.Parameters.Add("@leaseDurationSeconds", SqlDbType.Int).Value =
            Math.Max(1, wikiOptions.LeaseDurationSeconds);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadJob(reader) : null;
    }

    public async Task<IReadOnlyCollection<WikiPageCandidate>> GetCandidatesAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        const string commandText = """
            SELECT TOP (@limit)
                page.Id,
                page.NormalizedKey,
                page.Title,
                page.PageType,
                page.CurrentContent
            FROM [dbo].[KnowledgeWikiPages] page
            ORDER BY page.UpdatedAt DESC, page.Id;
            """;
        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@limit", SqlDbType.Int).Value = Math.Max(0, limit);
        var pages = new List<WikiPageCandidate>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pages.Add(new WikiPageCandidate(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                []));
        }

        await reader.CloseAsync();
        for (var index = 0; index < pages.Count; index++)
        {
            var page = pages[index];
            pages[index] = page with
            {
                Claims = await ReadCandidateClaimsAsync(connection, page.Id, cancellationToken)
            };
        }

        return pages;
    }

    public async Task ApplyAnalysisAsync(
        WikiProcessingJob job,
        WikiAnalysisResult analysis,
        IReadOnlyDictionary<string, EmbeddingVector> pageEmbeddings,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            if (!await TryLockActiveJobAsync(connection, transaction, job, cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var pages = await ReadPageMapAsync(connection, transaction, cancellationToken);
            var revisionByPage = new Dictionary<Guid, WikiRevision>();
            var now = DateTime.UtcNow;

            foreach (var proposal in analysis.Pages)
            {
                var page = ResolvePage(proposal, pages);
                var previousRevision = page is null || page.CurrentRevision == 0
                    ? null
                    : await GetCurrentRevisionAsync(
                        connection,
                        transaction,
                        page.Id,
                        cancellationToken);
                if (page is null)
                {
                    page = new WikiPage(
                        Guid.CreateVersion7(),
                        proposal.Key,
                        proposal.Title,
                        proposal.PageType,
                        proposal.Scope,
                        0,
                        string.Empty,
                        now,
                        now);
                    await InsertPageAsync(connection, transaction, page, cancellationToken);
                }

                var revision = new WikiRevision(
                    Guid.CreateVersion7(),
                    page.Id,
                    page.CurrentRevision + 1,
                    proposal.Content,
                    job.DocumentId,
                    job.Id,
                    now);
                page = page with
                {
                    Title = proposal.Title,
                    PageType = proposal.PageType,
                    Scope = proposal.Scope,
                    CurrentRevision = revision.RevisionNumber,
                    Content = proposal.Content,
                    UpdatedAt = now
                };

                await UpdatePageAsync(connection, transaction, page, cancellationToken);
                await InsertRevisionAsync(connection, transaction, revision, cancellationToken);
                revisionByPage[page.Id] = revision;
                pages[proposal.Key] = page;
                pages[page.NormalizedKey] = page;

                var sequence = previousRevision is null
                    ? 0
                    : await CopyUnchangedClaimsAsync(
                        connection,
                        transaction,
                        previousRevision.Id,
                        revision,
                        proposal.Claims.Select(claim => claim.Key).ToHashSet(StringComparer.Ordinal),
                        cancellationToken);
                foreach (var claimProposal in proposal.Claims)
                {
                    var claim = new WikiClaim(
                        Guid.CreateVersion7(),
                        revision.Id,
                        claimProposal.Key,
                        claimProposal.Content,
                        sequence++);
                    await InsertClaimAsync(connection, transaction, claim, cancellationToken);
                    foreach (var chunkId in claimProposal.EvidenceChunkIds)
                    {
                        await InsertEvidenceAsync(
                            connection,
                            transaction,
                            new WikiEvidence(
                                Guid.CreateVersion7(),
                                page.Id,
                                revision.Id,
                                claim.Id,
                                job.DocumentId,
                                chunkId,
                                WikiEvidenceStatus.Available,
                                now),
                            cancellationToken);
                    }
                }

                if (pageEmbeddings.TryGetValue(proposal.Key, out var embedding))
                {
                    ValidateDimensions(embedding);
                    await UpsertEmbeddingAsync(
                        connection,
                        transaction,
                        page,
                        embedding,
                        cancellationToken);
                }
            }

            foreach (var proposal in analysis.Links)
            {
                var source = pages[proposal.SourcePageKey];
                var target = pages[proposal.TargetPageKey];
                var revision = revisionByPage.GetValueOrDefault(source.Id)
                               ?? await GetCurrentRevisionAsync(
                                   connection,
                                   transaction,
                                   source.Id,
                                   cancellationToken);
                await InsertLinkAsync(
                    connection,
                    transaction,
                    new WikiLink(
                        Guid.CreateVersion7(),
                        source.Id,
                        target.Id,
                        proposal.RelationshipType,
                        revision.Id,
                        now),
                    cancellationToken);
            }

            foreach (var proposal in analysis.Issues)
            {
                var page = pages[proposal.PageKey];
                var revision = revisionByPage.GetValueOrDefault(page.Id)
                               ?? await GetCurrentRevisionAsync(
                                   connection,
                                   transaction,
                                   page.Id,
                                   cancellationToken);
                var claimId = await FindClaimIdAsync(
                    connection,
                    transaction,
                    revision.Id,
                    proposal.ClaimKey,
                    cancellationToken);
                await InsertIssueAsync(
                    connection,
                    transaction,
                    new WikiIssue(
                        Guid.CreateVersion7(),
                        page.Id,
                        claimId,
                        proposal.Type,
                        WikiIssueStatus.Open,
                        proposal.Description,
                        job.DocumentId,
                        now,
                        null),
                    cancellationToken);
            }

            await CompleteJobAsync(connection, transaction, job.Id, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task MarkJobFailedAsync(
        WikiProcessingJob job,
        string errorType,
        DateTime? nextAttemptAt,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        const string commandText = """
            UPDATE [dbo].[KnowledgeWikiProcessingJobs]
            SET Status = @status,
                UpdatedAt = SYSUTCDATETIME(),
                NextAttemptAt = @nextAttemptAt,
                ErrorType = @errorType
            WHERE Id = @id
              AND Status = 'Processing'
              AND UpdatedAt = @leasedAt;
            """;
        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = job.Id;
        command.Parameters.Add("@leasedAt", SqlDbType.DateTime2).Value = job.UpdatedAt;
        command.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value =
            nextAttemptAt is null ? "Failed" : "Pending";
        command.Parameters.Add("@nextAttemptAt", SqlDbType.DateTime2).Value =
            nextAttemptAt is null ? DBNull.Value : nextAttemptAt.Value;
        command.Parameters.Add("@errorType", SqlDbType.NVarChar, 500).Value = errorType;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkEvidenceUnavailableAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        const string commandText = """
            UPDATE [dbo].[KnowledgeWikiEvidence]
            SET Status = 'Unavailable'
            WHERE DocumentId = @documentId AND Status <> 'Unavailable';

            UPDATE [dbo].[KnowledgeWikiProcessingJobs]
            SET Status = 'Failed',
                UpdatedAt = SYSUTCDATETIME(),
                NextAttemptAt = NULL,
                ErrorType = 'SourceDocumentChanged'
            WHERE DocumentId = @documentId
              AND Status IN ('Pending', 'Processing');
            """;
        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = documentId;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<WikiProcessingJob?> GetJobByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        const string commandText = """
            SELECT Id, DocumentId, Status, AttemptCount, CreatedAt, UpdatedAt, NextAttemptAt, ErrorType
            FROM [dbo].[KnowledgeWikiProcessingJobs]
            WHERE DocumentId = @documentId
            ORDER BY CreatedAt DESC, Id DESC;
            """;
        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = documentId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadJob(reader) : null;
    }

    public async Task<WikiPageResult?> GetPageAsync(
        string normalizedKey,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        var page = await ReadPageAsync(connection, normalizedKey, cancellationToken);
        if (page is null)
            return null;

        var claims = await ReadClaimsAsync(connection, page.Id, page.CurrentRevision, cancellationToken);
        var links = await ReadLinksAsync(connection, page.Id, cancellationToken);
        var issues = await ReadIssuesAsync(connection, page.Id, cancellationToken);
        return new WikiPageResult(page, claims, links, issues);
    }

    public async Task<WikiPageEvidenceResult?> GetPageEvidenceAsync(
        Guid pageId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        const string commandText = """
            SELECT
                evidence.Id,
                evidence.PageId,
                evidence.RevisionId,
                evidence.ClaimId,
                evidence.DocumentId,
                evidence.ChunkId,
                evidence.Status,
                evidence.AttachedAt,
                claim.ClaimContent,
                document.Title,
                chunk.Sequence,
                chunk.KnowledgeContent
            FROM [dbo].[KnowledgeWikiEvidence] evidence
            INNER JOIN [dbo].[KnowledgeWikiClaims] claim ON claim.Id = evidence.ClaimId
            INNER JOIN [dbo].[KnowledgeWikiPages] page ON page.Id = evidence.PageId
            INNER JOIN [dbo].[KnowledgeWikiRevisions] revision
                ON revision.PageId = page.Id
               AND revision.RevisionNumber = page.CurrentRevision
               AND revision.Id = evidence.RevisionId
            LEFT JOIN [dbo].[KnowledgeDocuments] document ON document.Id = evidence.DocumentId
            LEFT JOIN [dbo].[KnowledgeChunks] chunk ON chunk.Id = evidence.ChunkId
            WHERE evidence.PageId = @pageId
            ORDER BY claim.Sequence, chunk.Sequence, evidence.Id;
            """;
        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = pageId;
        var items = new List<WikiPageEvidenceItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var evidence = new WikiEvidence(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetGuid(2),
                reader.GetGuid(3),
                reader.GetGuid(4),
                reader.GetGuid(5),
                Enum.Parse<WikiEvidenceStatus>(reader.GetString(6)),
                reader.GetDateTime(7));
            items.Add(new WikiPageEvidenceItem(
                evidence,
                reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9),
                reader.IsDBNull(10) ? null : reader.GetInt32(10),
                evidence.Status == WikiEvidenceStatus.Available && !reader.IsDBNull(11)
                    ? reader.GetString(11)
                    : null));
        }

        if (items.Count > 0)
            return new WikiPageEvidenceResult(pageId, items);

        await using var exists = new SqlCommand(
            "SELECT COUNT(*) FROM [dbo].[KnowledgeWikiPages] WHERE Id = @pageId;",
            connection);
        exists.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = pageId;
        return Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken)) == 0
            ? null
            : new WikiPageEvidenceResult(pageId, []);
    }

    public async Task<IReadOnlyCollection<(WikiPage Page, double Score)>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int topK,
        CancellationToken cancellationToken)
    {
        ValidateDimensions(queryEmbedding);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        var commandText = $"""
            SELECT TOP (@topK)
                page.Id,
                page.NormalizedKey,
                page.Title,
                page.PageType,
                page.Scope,
                page.CurrentRevision,
                page.CurrentContent,
                page.CreatedAt,
                page.UpdatedAt,
                1.0 - VECTOR_DISTANCE(
                    'cosine',
                    CAST(@embedding AS VECTOR({options.Dimensions})),
                    vector.Embedding) AS Score
            FROM [dbo].[KnowledgeWikiPages] page
            INNER JOIN [dbo].[KnowledgeWikiPageVectors] vector ON vector.PageId = page.Id
            WHERE @scope IS NULL OR page.Scope = @scope
            ORDER BY Score DESC, page.Title, page.Id;
            """;
        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@topK", SqlDbType.Int).Value = topK;
        command.Parameters.Add("@scope", SqlDbType.NVarChar, 50).Value =
            scope is null ? DBNull.Value : scope.Name;
        command.Parameters.Add("@embedding", SqlDbType.NVarChar, -1).Value =
            JsonSerializer.Serialize(queryEmbedding.Values);
        var result = new List<(WikiPage Page, double Score)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add((ReadPage(reader), reader.GetDouble(9)));
        return result;
    }

    private async Task EnsureSchemaAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
        => await SqlServerKnowledgeSchema.EnsureCreatedAsync(
            connection,
            options.Dimensions,
            cancellationToken);

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new InvalidOperationException(
                "A SQL Server connection string is required for Knowledge Wiki persistence.");

        var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private void ValidateDimensions(EmbeddingVector embedding)
    {
        if (embedding.Dimensions != options.Dimensions)
            throw new InvalidOperationException(
                $"Expected a Wiki embedding with {options.Dimensions} dimensions, " +
                $"but received {embedding.Dimensions}.");
    }

    private static async Task<bool> TryLockActiveJobAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiProcessingJob job,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(
            """
            SELECT COUNT(1)
            FROM [dbo].[KnowledgeWikiProcessingJobs] WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @id
              AND Status = 'Processing'
              AND UpdatedAt = @leasedAt;
            """,
            connection,
            transaction);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = job.Id;
        command.Parameters.Add("@leasedAt", SqlDbType.DateTime2).Value = job.UpdatedAt;
        return (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0) == 1;
    }

    private static async Task<IReadOnlyCollection<string>> ReadCandidateClaimsAsync(
        SqlConnection connection,
        Guid pageId,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(
            """
            SELECT claim.ClaimContent
            FROM [dbo].[KnowledgeWikiClaims] claim
            INNER JOIN [dbo].[KnowledgeWikiRevisions] revision ON revision.Id = claim.RevisionId
            INNER JOIN [dbo].[KnowledgeWikiPages] page
                ON page.Id = revision.PageId
               AND page.CurrentRevision = revision.RevisionNumber
            WHERE page.Id = @pageId
            ORDER BY claim.Sequence, claim.Id;
            """,
            connection);
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = pageId;
        var claims = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            claims.Add(reader.GetString(0));
        return claims;
    }

    private static async Task<int> CopyUnchangedClaimsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid previousRevisionId,
        WikiRevision newRevision,
        IReadOnlySet<string> replacedClaimKeys,
        CancellationToken cancellationToken)
    {
        const string commandText = """
            SELECT
                claim.Id,
                claim.NormalizedKey,
                claim.ClaimContent,
                claim.Sequence,
                evidence.DocumentId,
                evidence.ChunkId,
                evidence.Status,
                evidence.AttachedAt
            FROM [dbo].[KnowledgeWikiClaims] claim
            LEFT JOIN [dbo].[KnowledgeWikiEvidence] evidence ON evidence.ClaimId = claim.Id
            WHERE claim.RevisionId = @revisionId
            ORDER BY claim.Sequence, claim.Id, evidence.Id;
            """;
        await using var command = new SqlCommand(commandText, connection, transaction);
        command.Parameters.Add("@revisionId", SqlDbType.UniqueIdentifier).Value = previousRevisionId;
        var claims = new List<(WikiClaim Claim, List<WikiEvidence> Evidence)>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var sourceClaimId = reader.GetGuid(0);
                var entry = claims.LastOrDefault(item => item.Claim.Id == sourceClaimId);
                if (entry.Claim is null)
                {
                    entry = (
                        new WikiClaim(
                            sourceClaimId,
                            previousRevisionId,
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetInt32(3)),
                        []);
                    claims.Add(entry);
                }

                if (!reader.IsDBNull(4))
                {
                    entry.Evidence.Add(new WikiEvidence(
                        Guid.Empty,
                        newRevision.PageId,
                        previousRevisionId,
                        sourceClaimId,
                        reader.GetGuid(4),
                        reader.GetGuid(5),
                        Enum.Parse<WikiEvidenceStatus>(reader.GetString(6)),
                        reader.GetDateTime(7)));
                }
            }
        }

        var sequence = 0;
        foreach (var source in claims.Where(
                     item => !replacedClaimKeys.Contains(item.Claim.NormalizedKey)))
        {
            var claim = new WikiClaim(
                Guid.CreateVersion7(),
                newRevision.Id,
                source.Claim.NormalizedKey,
                source.Claim.Content,
                sequence++);
            await InsertClaimAsync(connection, transaction, claim, cancellationToken);
            foreach (var sourceEvidence in source.Evidence)
            {
                await InsertEvidenceAsync(
                    connection,
                    transaction,
                    new WikiEvidence(
                        Guid.CreateVersion7(),
                        newRevision.PageId,
                        newRevision.Id,
                        claim.Id,
                        sourceEvidence.DocumentId,
                        sourceEvidence.ChunkId,
                        sourceEvidence.Status,
                        sourceEvidence.AttachedAt),
                    cancellationToken);
            }
        }

        return sequence;
    }

    private static async Task<Dictionary<string, WikiPage>> ReadPageMapAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(
            """
            SELECT Id, NormalizedKey, Title, PageType, Scope, CurrentRevision,
                   CurrentContent, CreatedAt, UpdatedAt
            FROM [dbo].[KnowledgeWikiPages] WITH (UPDLOCK);
            """,
            connection,
            transaction);
        var pages = new Dictionary<string, WikiPage>(StringComparer.Ordinal);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var page = ReadPage(reader);
            pages[page.NormalizedKey] = page;
        }

        return pages;
    }

    private static WikiPage? ResolvePage(
        WikiPageProposal proposal,
        IReadOnlyDictionary<string, WikiPage> pages)
    {
        if (proposal.ExistingPageId is { } pageId)
        {
            var page = pages.Values.FirstOrDefault(candidate => candidate.Id == pageId);
            if (page is not null)
                return page;
        }

        return pages.GetValueOrDefault(proposal.Key);
    }

    private static async Task InsertPageAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiPage page,
        CancellationToken cancellationToken)
    {
        const string text = """
            INSERT INTO [dbo].[KnowledgeWikiPages]
                (Id, NormalizedKey, Title, PageType, Scope, CurrentRevision,
                 CurrentContent, CreatedAt, UpdatedAt)
            VALUES
                (@id, @key, @title, @pageType, @scope, @revision,
                 @content, @createdAt, @updatedAt);
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        AddPageParameters(command, page);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpdatePageAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiPage page,
        CancellationToken cancellationToken)
    {
        const string text = """
            UPDATE [dbo].[KnowledgeWikiPages]
            SET Title = @title,
                PageType = @pageType,
                Scope = @scope,
                CurrentRevision = @revision,
                CurrentContent = @content,
                UpdatedAt = @updatedAt
            WHERE Id = @id;
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        AddPageParameters(command, page);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddPageParameters(SqlCommand command, WikiPage page)
    {
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = page.Id;
        command.Parameters.Add("@key", SqlDbType.NVarChar, 300).Value = page.NormalizedKey;
        command.Parameters.Add("@title", SqlDbType.NVarChar, 300).Value = page.Title;
        command.Parameters.Add("@pageType", SqlDbType.NVarChar, 100).Value = page.PageType;
        command.Parameters.Add("@scope", SqlDbType.NVarChar, 50).Value = page.Scope.Name;
        command.Parameters.Add("@revision", SqlDbType.Int).Value = page.CurrentRevision;
        command.Parameters.Add("@content", SqlDbType.NVarChar, -1).Value = page.Content;
        command.Parameters.Add("@createdAt", SqlDbType.DateTime2).Value = page.CreatedAt;
        command.Parameters.Add("@updatedAt", SqlDbType.DateTime2).Value = page.UpdatedAt;
    }

    private static async Task InsertRevisionAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiRevision revision,
        CancellationToken cancellationToken)
    {
        const string text = """
            INSERT INTO [dbo].[KnowledgeWikiRevisions]
                (Id, PageId, RevisionNumber, RevisionContent, SourceDocumentId,
                 ProcessingJobId, CreatedAt)
            VALUES
                (@id, @pageId, @number, @content, @documentId, @jobId, @createdAt);
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = revision.Id;
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = revision.PageId;
        command.Parameters.Add("@number", SqlDbType.Int).Value = revision.RevisionNumber;
        command.Parameters.Add("@content", SqlDbType.NVarChar, -1).Value = revision.Content;
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = revision.SourceDocumentId;
        command.Parameters.Add("@jobId", SqlDbType.UniqueIdentifier).Value = revision.ProcessingJobId;
        command.Parameters.Add("@createdAt", SqlDbType.DateTime2).Value = revision.CreatedAt;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertClaimAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiClaim claim,
        CancellationToken cancellationToken)
    {
        const string text = """
            INSERT INTO [dbo].[KnowledgeWikiClaims]
                (Id, RevisionId, NormalizedKey, ClaimContent, Sequence)
            VALUES
                (@id, @revisionId, @key, @content, @sequence);
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = claim.Id;
        command.Parameters.Add("@revisionId", SqlDbType.UniqueIdentifier).Value = claim.RevisionId;
        command.Parameters.Add("@key", SqlDbType.NVarChar, 300).Value = claim.NormalizedKey;
        command.Parameters.Add("@content", SqlDbType.NVarChar, -1).Value = claim.Content;
        command.Parameters.Add("@sequence", SqlDbType.Int).Value = claim.Sequence;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertEvidenceAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiEvidence evidence,
        CancellationToken cancellationToken)
    {
        const string text = """
            INSERT INTO [dbo].[KnowledgeWikiEvidence]
                (Id, PageId, RevisionId, ClaimId, DocumentId, ChunkId, Status, AttachedAt)
            VALUES
                (@id, @pageId, @revisionId, @claimId, @documentId, @chunkId, @status, @attachedAt);
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = evidence.Id;
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = evidence.PageId;
        command.Parameters.Add("@revisionId", SqlDbType.UniqueIdentifier).Value = evidence.RevisionId;
        command.Parameters.Add("@claimId", SqlDbType.UniqueIdentifier).Value = evidence.ClaimId;
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = evidence.DocumentId;
        command.Parameters.Add("@chunkId", SqlDbType.UniqueIdentifier).Value = evidence.ChunkId;
        command.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value = evidence.Status.ToString();
        command.Parameters.Add("@attachedAt", SqlDbType.DateTime2).Value = evidence.AttachedAt;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task UpsertEmbeddingAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiPage page,
        EmbeddingVector embedding,
        CancellationToken cancellationToken)
    {
        var text = $"""
            UPDATE [dbo].[KnowledgeWikiPageVectors]
            SET Scope = @scope,
                Embedding = CAST(@embedding AS VECTOR({options.Dimensions}))
            WHERE PageId = @pageId;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO [dbo].[KnowledgeWikiPageVectors] (PageId, Scope, Embedding)
                VALUES (@pageId, @scope, CAST(@embedding AS VECTOR({options.Dimensions})));
            END;
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = page.Id;
        command.Parameters.Add("@scope", SqlDbType.NVarChar, 50).Value = page.Scope.Name;
        command.Parameters.Add("@embedding", SqlDbType.NVarChar, -1).Value =
            JsonSerializer.Serialize(embedding.Values);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertLinkAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiLink link,
        CancellationToken cancellationToken)
    {
        const string text = """
            IF NOT EXISTS
            (
                SELECT 1
                FROM [dbo].[KnowledgeWikiLinks]
                WHERE SourcePageId = @sourcePageId
                  AND TargetPageId = @targetPageId
                  AND RelationshipType = @relationshipType
            )
            BEGIN
                INSERT INTO [dbo].[KnowledgeWikiLinks]
                    (Id, SourcePageId, TargetPageId, RelationshipType, RevisionId, CreatedAt)
                VALUES
                    (@id, @sourcePageId, @targetPageId, @relationshipType, @revisionId, @createdAt);
            END;
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = link.Id;
        command.Parameters.Add("@sourcePageId", SqlDbType.UniqueIdentifier).Value = link.SourcePageId;
        command.Parameters.Add("@targetPageId", SqlDbType.UniqueIdentifier).Value = link.TargetPageId;
        command.Parameters.Add("@relationshipType", SqlDbType.NVarChar, 100).Value = link.RelationshipType;
        command.Parameters.Add("@revisionId", SqlDbType.UniqueIdentifier).Value = link.RevisionId;
        command.Parameters.Add("@createdAt", SqlDbType.DateTime2).Value = link.CreatedAt;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertIssueAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        WikiIssue issue,
        CancellationToken cancellationToken)
    {
        const string text = """
            INSERT INTO [dbo].[KnowledgeWikiIssues]
                (Id, PageId, ClaimId, IssueType, Status, Description,
                 SourceDocumentId, CreatedAt, ResolvedAt)
            VALUES
                (@id, @pageId, @claimId, @issueType, @status, @description,
                 @documentId, @createdAt, @resolvedAt);
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = issue.Id;
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = issue.PageId;
        command.Parameters.Add("@claimId", SqlDbType.UniqueIdentifier).Value =
            issue.ClaimId is null ? DBNull.Value : issue.ClaimId.Value;
        command.Parameters.Add("@issueType", SqlDbType.NVarChar, 50).Value = issue.Type.ToString();
        command.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value = issue.Status.ToString();
        command.Parameters.Add("@description", SqlDbType.NVarChar, 2000).Value = issue.Description;
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = issue.SourceDocumentId;
        command.Parameters.Add("@createdAt", SqlDbType.DateTime2).Value = issue.CreatedAt;
        command.Parameters.Add("@resolvedAt", SqlDbType.DateTime2).Value =
            issue.ResolvedAt is null ? DBNull.Value : issue.ResolvedAt.Value;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task CompleteJobAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid jobId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string text = """
            UPDATE [dbo].[KnowledgeWikiProcessingJobs]
            SET Status = 'Completed',
                UpdatedAt = @now,
                NextAttemptAt = NULL,
                ErrorType = NULL
            WHERE Id = @id;
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = jobId;
        command.Parameters.Add("@now", SqlDbType.DateTime2).Value = now;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<WikiRevision> GetCurrentRevisionAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid pageId,
        CancellationToken cancellationToken)
    {
        const string text = """
            SELECT revision.Id, revision.PageId, revision.RevisionNumber,
                   revision.RevisionContent, revision.SourceDocumentId,
                   revision.ProcessingJobId, revision.CreatedAt
            FROM [dbo].[KnowledgeWikiRevisions] revision
            INNER JOIN [dbo].[KnowledgeWikiPages] page
                ON page.Id = revision.PageId
               AND page.CurrentRevision = revision.RevisionNumber
            WHERE page.Id = @pageId;
            """;
        await using var command = new SqlCommand(text, connection, transaction);
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = pageId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException($"Wiki page '{pageId}' has no current revision.");
        return new WikiRevision(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetInt32(2),
            reader.GetString(3),
            reader.GetGuid(4),
            reader.GetGuid(5),
            reader.GetDateTime(6));
    }

    private static async Task<Guid?> FindClaimIdAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid revisionId,
        string? claimKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(claimKey))
            return null;

        await using var command = new SqlCommand(
            """
            SELECT Id
            FROM [dbo].[KnowledgeWikiClaims]
            WHERE RevisionId = @revisionId AND NormalizedKey = @claimKey;
            """,
            connection,
            transaction);
        command.Parameters.Add("@revisionId", SqlDbType.UniqueIdentifier).Value = revisionId;
        command.Parameters.Add("@claimKey", SqlDbType.NVarChar, 300).Value = claimKey;
        return await command.ExecuteScalarAsync(cancellationToken) as Guid?;
    }

    private static async Task<WikiPage?> ReadPageAsync(
        SqlConnection connection,
        string key,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(
            """
            SELECT Id, NormalizedKey, Title, PageType, Scope, CurrentRevision,
                   CurrentContent, CreatedAt, UpdatedAt
            FROM [dbo].[KnowledgeWikiPages]
            WHERE NormalizedKey = @key;
            """,
            connection);
        command.Parameters.Add("@key", SqlDbType.NVarChar, 300).Value = key;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadPage(reader) : null;
    }

    private static WikiPage ReadPage(SqlDataReader reader)
        => new(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            DocumentScope.FromName(reader.GetString(4)),
            reader.GetInt32(5),
            reader.GetString(6),
            reader.GetDateTime(7),
            reader.GetDateTime(8));

    private static async Task<IReadOnlyCollection<WikiClaim>> ReadClaimsAsync(
        SqlConnection connection,
        Guid pageId,
        int revisionNumber,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(
            """
            SELECT claim.Id, claim.RevisionId, claim.NormalizedKey,
                   claim.ClaimContent, claim.Sequence
            FROM [dbo].[KnowledgeWikiClaims] claim
            INNER JOIN [dbo].[KnowledgeWikiRevisions] revision ON revision.Id = claim.RevisionId
            WHERE revision.PageId = @pageId AND revision.RevisionNumber = @revision
            ORDER BY claim.Sequence, claim.Id;
            """,
            connection);
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = pageId;
        command.Parameters.Add("@revision", SqlDbType.Int).Value = revisionNumber;
        var result = new List<WikiClaim>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new WikiClaim(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4)));
        return result;
    }

    private static async Task<IReadOnlyCollection<WikiLink>> ReadLinksAsync(
        SqlConnection connection,
        Guid pageId,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(
            """
            SELECT Id, SourcePageId, TargetPageId, RelationshipType, RevisionId, CreatedAt
            FROM [dbo].[KnowledgeWikiLinks]
            WHERE SourcePageId = @pageId
            ORDER BY RelationshipType, TargetPageId;
            """,
            connection);
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = pageId;
        var result = new List<WikiLink>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new WikiLink(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetGuid(2),
                reader.GetString(3),
                reader.GetGuid(4),
                reader.GetDateTime(5)));
        return result;
    }

    private static async Task<IReadOnlyCollection<WikiIssue>> ReadIssuesAsync(
        SqlConnection connection,
        Guid pageId,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(
            """
            SELECT Id, PageId, ClaimId, IssueType, Status, Description,
                   SourceDocumentId, CreatedAt, ResolvedAt
            FROM [dbo].[KnowledgeWikiIssues]
            WHERE PageId = @pageId
            ORDER BY CreatedAt DESC, Id;
            """,
            connection);
        command.Parameters.Add("@pageId", SqlDbType.UniqueIdentifier).Value = pageId;
        var result = new List<WikiIssue>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new WikiIssue(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.IsDBNull(2) ? null : reader.GetGuid(2),
                Enum.Parse<WikiIssueType>(reader.GetString(3)),
                Enum.Parse<WikiIssueStatus>(reader.GetString(4)),
                reader.GetString(5),
                reader.GetGuid(6),
                reader.GetDateTime(7),
                reader.IsDBNull(8) ? null : reader.GetDateTime(8)));
        return result;
    }

    private static WikiProcessingJob ReadJob(SqlDataReader reader)
        => new(
            reader.GetGuid(0),
            reader.GetGuid(1),
            Enum.Parse<WikiProcessingStatus>(reader.GetString(2)),
            reader.GetInt32(3),
            reader.GetDateTime(4),
            reader.GetDateTime(5),
            reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            reader.IsDBNull(7) ? null : reader.GetString(7));
}
