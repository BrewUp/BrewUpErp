using Microsoft.Data.SqlClient;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

internal static class SqlServerKnowledgeSchema
{
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);

    public static async Task EnsureCreatedAsync(
        SqlConnection connection,
        int vectorDimensions,
        CancellationToken cancellationToken)
    {
        if (vectorDimensions is < 1 or > 1998)
            throw new InvalidOperationException(
                "SQL Server vector dimensions must be between 1 and 1998.");

        await SchemaLock.WaitAsync(cancellationToken);
        try
        {
            var commandText = $"""
                IF OBJECT_ID(N'dbo.KnowledgeDocuments', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeDocuments]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeDocuments PRIMARY KEY,
                        Title NVARCHAR(300) NOT NULL,
                        DocumentsContent NVARCHAR(MAX) NOT NULL,
                        Source NVARCHAR(50) NOT NULL,
                        Scope NVARCHAR(50) NOT NULL,
                        Tags NVARCHAR(MAX) NOT NULL,
                        ImportedAt DATETIME2 NOT NULL
                    );
                END;

                IF COL_LENGTH(N'dbo.KnowledgeDocuments', N'DocumentsContent') IS NULL
                   AND COL_LENGTH(N'dbo.KnowledgeDocuments', N'Content') IS NOT NULL
                BEGIN
                    EXEC sp_rename
                        N'dbo.KnowledgeDocuments.Content',
                        N'DocumentsContent',
                        N'COLUMN';
                END;

                IF OBJECT_ID(N'dbo.KnowledgeChunks', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeChunks]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeChunks PRIMARY KEY,
                        DocumentId UNIQUEIDENTIFIER NOT NULL,
                        Sequence INT NOT NULL,
                        KnowledgeContent NVARCHAR(MAX) NOT NULL,
                        TokenCount INT NOT NULL,
                        Scope NVARCHAR(50) NOT NULL,
                        Title NVARCHAR(300) NOT NULL,
                        Tags NVARCHAR(MAX) NOT NULL,
                        CONSTRAINT FK_KnowledgeChunks_KnowledgeDocuments
                            FOREIGN KEY (DocumentId)
                            REFERENCES [dbo].[KnowledgeDocuments] (Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IX_KnowledgeChunks_DocumentId_Sequence
                        ON [dbo].[KnowledgeChunks] (DocumentId, Sequence);
                END;

                IF COL_LENGTH(N'dbo.KnowledgeChunks', N'KnowledgeContent') IS NULL
                   AND COL_LENGTH(N'dbo.KnowledgeChunks', N'Content') IS NOT NULL
                BEGIN
                    EXEC sp_rename
                        N'dbo.KnowledgeChunks.Content',
                        N'KnowledgeContent',
                        N'COLUMN';
                END;

                IF OBJECT_ID(N'dbo.KnowledgeVectors', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeVectors]
                    (
                        ChunkId UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeVectors PRIMARY KEY,
                        DocumentId UNIQUEIDENTIFIER NOT NULL,
                        Sequence INT NOT NULL,
                        Title NVARCHAR(500) NOT NULL,
                        Scope NVARCHAR(50) NOT NULL,
                        Tags NVARCHAR(MAX) NOT NULL,
                        VectorsContent NVARCHAR(MAX) NOT NULL,
                        TokenCount INT NOT NULL,
                        MaxCharacters INT NOT NULL,
                        OverlapCharacters INT NOT NULL,
                        Embedding VECTOR({vectorDimensions}) NOT NULL,
                        CONSTRAINT FK_KnowledgeVectors_KnowledgeChunks
                            FOREIGN KEY (ChunkId)
                            REFERENCES [dbo].[KnowledgeChunks] (Id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IX_KnowledgeVectors_DocumentId
                        ON [dbo].[KnowledgeVectors] (DocumentId, Sequence);

                    CREATE INDEX IX_KnowledgeVectors_Scope
                        ON [dbo].[KnowledgeVectors] (Scope);
                END;

                IF COL_LENGTH(N'dbo.KnowledgeVectors', N'VectorsContent') IS NULL
                   AND COL_LENGTH(N'dbo.KnowledgeVectors', N'Content') IS NOT NULL
                BEGIN
                    EXEC sp_rename
                        N'dbo.KnowledgeVectors.Content',
                        N'VectorsContent',
                        N'COLUMN';
                END;

                IF OBJECT_ID(N'dbo.KnowledgeWikiPages', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeWikiPages]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeWikiPages PRIMARY KEY,
                        NormalizedKey NVARCHAR(300) NOT NULL,
                        Title NVARCHAR(300) NOT NULL,
                        PageType NVARCHAR(100) NOT NULL,
                        Scope NVARCHAR(50) NOT NULL,
                        CurrentRevision INT NOT NULL,
                        CurrentContent NVARCHAR(MAX) NOT NULL,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        CONSTRAINT UQ_KnowledgeWikiPages_NormalizedKey UNIQUE (NormalizedKey)
                    );
                END;

                IF OBJECT_ID(N'dbo.KnowledgeWikiProcessingJobs', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeWikiProcessingJobs]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeWikiProcessingJobs PRIMARY KEY,
                        DocumentId UNIQUEIDENTIFIER NOT NULL,
                        Status NVARCHAR(30) NOT NULL,
                        AttemptCount INT NOT NULL,
                        CreatedAt DATETIME2 NOT NULL,
                        UpdatedAt DATETIME2 NOT NULL,
                        NextAttemptAt DATETIME2 NULL,
                        ErrorType NVARCHAR(500) NULL
                    );

                    CREATE INDEX IX_KnowledgeWikiProcessingJobs_Status_NextAttemptAt
                        ON [dbo].[KnowledgeWikiProcessingJobs] (Status, NextAttemptAt, CreatedAt);
                END;

                IF EXISTS
                (
                    SELECT 1
                    FROM sys.key_constraints
                    WHERE [name] = N'UQ_KnowledgeWikiProcessingJobs_DocumentId'
                      AND parent_object_id = OBJECT_ID(N'dbo.KnowledgeWikiProcessingJobs')
                )
                BEGIN
                    ALTER TABLE [dbo].[KnowledgeWikiProcessingJobs]
                        DROP CONSTRAINT UQ_KnowledgeWikiProcessingJobs_DocumentId;
                END;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_KnowledgeWikiProcessingJobs_DocumentId_CreatedAt'
                      AND object_id = OBJECT_ID(N'dbo.KnowledgeWikiProcessingJobs')
                )
                BEGIN
                    CREATE INDEX IX_KnowledgeWikiProcessingJobs_DocumentId_CreatedAt
                        ON [dbo].[KnowledgeWikiProcessingJobs] (DocumentId, CreatedAt DESC);
                END;

                IF OBJECT_ID(N'dbo.KnowledgeWikiRevisions', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeWikiRevisions]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeWikiRevisions PRIMARY KEY,
                        PageId UNIQUEIDENTIFIER NOT NULL,
                        RevisionNumber INT NOT NULL,
                        RevisionContent NVARCHAR(MAX) NOT NULL,
                        SourceDocumentId UNIQUEIDENTIFIER NOT NULL,
                        ProcessingJobId UNIQUEIDENTIFIER NOT NULL,
                        CreatedAt DATETIME2 NOT NULL,
                        CONSTRAINT FK_KnowledgeWikiRevisions_Page
                            FOREIGN KEY (PageId)
                            REFERENCES [dbo].[KnowledgeWikiPages] (Id),
                        CONSTRAINT FK_KnowledgeWikiRevisions_Job
                            FOREIGN KEY (ProcessingJobId)
                            REFERENCES [dbo].[KnowledgeWikiProcessingJobs] (Id),
                        CONSTRAINT UQ_KnowledgeWikiRevisions_Page_Revision
                            UNIQUE (PageId, RevisionNumber),
                        CONSTRAINT UQ_KnowledgeWikiRevisions_Job_Page
                            UNIQUE (ProcessingJobId, PageId)
                    );
                END;

                IF OBJECT_ID(N'dbo.KnowledgeWikiClaims', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeWikiClaims]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeWikiClaims PRIMARY KEY,
                        RevisionId UNIQUEIDENTIFIER NOT NULL,
                        NormalizedKey NVARCHAR(300) NOT NULL,
                        ClaimContent NVARCHAR(MAX) NOT NULL,
                        Sequence INT NOT NULL,
                        CONSTRAINT FK_KnowledgeWikiClaims_Revision
                            FOREIGN KEY (RevisionId)
                            REFERENCES [dbo].[KnowledgeWikiRevisions] (Id),
                        CONSTRAINT UQ_KnowledgeWikiClaims_Revision_Key
                            UNIQUE (RevisionId, NormalizedKey)
                    );
                END;

                IF OBJECT_ID(N'dbo.KnowledgeWikiEvidence', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeWikiEvidence]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeWikiEvidence PRIMARY KEY,
                        PageId UNIQUEIDENTIFIER NOT NULL,
                        RevisionId UNIQUEIDENTIFIER NOT NULL,
                        ClaimId UNIQUEIDENTIFIER NOT NULL,
                        DocumentId UNIQUEIDENTIFIER NOT NULL,
                        ChunkId UNIQUEIDENTIFIER NOT NULL,
                        Status NVARCHAR(30) NOT NULL,
                        AttachedAt DATETIME2 NOT NULL,
                        CONSTRAINT FK_KnowledgeWikiEvidence_Page
                            FOREIGN KEY (PageId)
                            REFERENCES [dbo].[KnowledgeWikiPages] (Id),
                        CONSTRAINT FK_KnowledgeWikiEvidence_Revision
                            FOREIGN KEY (RevisionId)
                            REFERENCES [dbo].[KnowledgeWikiRevisions] (Id),
                        CONSTRAINT FK_KnowledgeWikiEvidence_Claim
                            FOREIGN KEY (ClaimId)
                            REFERENCES [dbo].[KnowledgeWikiClaims] (Id),
                        CONSTRAINT UQ_KnowledgeWikiEvidence_Claim_Chunk
                            UNIQUE (ClaimId, ChunkId)
                    );

                    CREATE INDEX IX_KnowledgeWikiEvidence_DocumentId
                        ON [dbo].[KnowledgeWikiEvidence] (DocumentId, Status);
                END;

                IF OBJECT_ID(N'dbo.KnowledgeWikiLinks', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeWikiLinks]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeWikiLinks PRIMARY KEY,
                        SourcePageId UNIQUEIDENTIFIER NOT NULL,
                        TargetPageId UNIQUEIDENTIFIER NOT NULL,
                        RelationshipType NVARCHAR(100) NOT NULL,
                        RevisionId UNIQUEIDENTIFIER NOT NULL,
                        CreatedAt DATETIME2 NOT NULL,
                        CONSTRAINT FK_KnowledgeWikiLinks_SourcePage
                            FOREIGN KEY (SourcePageId)
                            REFERENCES [dbo].[KnowledgeWikiPages] (Id),
                        CONSTRAINT FK_KnowledgeWikiLinks_TargetPage
                            FOREIGN KEY (TargetPageId)
                            REFERENCES [dbo].[KnowledgeWikiPages] (Id),
                        CONSTRAINT FK_KnowledgeWikiLinks_Revision
                            FOREIGN KEY (RevisionId)
                            REFERENCES [dbo].[KnowledgeWikiRevisions] (Id),
                        CONSTRAINT UQ_KnowledgeWikiLinks_Relationship
                            UNIQUE (SourcePageId, TargetPageId, RelationshipType)
                    );
                END;

                IF OBJECT_ID(N'dbo.KnowledgeWikiIssues', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeWikiIssues]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeWikiIssues PRIMARY KEY,
                        PageId UNIQUEIDENTIFIER NOT NULL,
                        ClaimId UNIQUEIDENTIFIER NULL,
                        IssueType NVARCHAR(50) NOT NULL,
                        Status NVARCHAR(30) NOT NULL,
                        Description NVARCHAR(2000) NOT NULL,
                        SourceDocumentId UNIQUEIDENTIFIER NOT NULL,
                        CreatedAt DATETIME2 NOT NULL,
                        ResolvedAt DATETIME2 NULL,
                        CONSTRAINT FK_KnowledgeWikiIssues_Page
                            FOREIGN KEY (PageId)
                            REFERENCES [dbo].[KnowledgeWikiPages] (Id),
                        CONSTRAINT FK_KnowledgeWikiIssues_Claim
                            FOREIGN KEY (ClaimId)
                            REFERENCES [dbo].[KnowledgeWikiClaims] (Id)
                    );
                END;

                IF OBJECT_ID(N'dbo.KnowledgeWikiPageVectors', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeWikiPageVectors]
                    (
                        PageId UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeWikiPageVectors PRIMARY KEY,
                        Scope NVARCHAR(50) NOT NULL,
                        Embedding VECTOR({vectorDimensions}) NOT NULL,
                        CONSTRAINT FK_KnowledgeWikiPageVectors_Page
                            FOREIGN KEY (PageId)
                            REFERENCES [dbo].[KnowledgeWikiPages] (Id)
                    );

                    CREATE INDEX IX_KnowledgeWikiPageVectors_Scope
                        ON [dbo].[KnowledgeWikiPageVectors] (Scope);
                END;
                """;

            await using var command = new SqlCommand(commandText, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            SchemaLock.Release();
        }
    }
}
