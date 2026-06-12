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
                        Content NVARCHAR(MAX) NOT NULL,
                        Source NVARCHAR(50) NOT NULL,
                        Scope NVARCHAR(50) NOT NULL,
                        Tags NVARCHAR(MAX) NOT NULL,
                        ImportedAt DATETIME2 NOT NULL
                    );
                END;

                IF OBJECT_ID(N'dbo.KnowledgeChunks', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KnowledgeChunks]
                    (
                        Id UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_KnowledgeChunks PRIMARY KEY,
                        DocumentId UNIQUEIDENTIFIER NOT NULL,
                        Sequence INT NOT NULL,
                        Content NVARCHAR(MAX) NOT NULL,
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
                        Content NVARCHAR(MAX) NOT NULL,
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
