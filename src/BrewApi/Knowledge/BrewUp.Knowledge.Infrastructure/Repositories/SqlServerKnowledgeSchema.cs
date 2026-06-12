using Microsoft.Data.SqlClient;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

internal static class SqlServerKnowledgeSchema
{
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);

    public static async Task EnsureCreatedAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        await SchemaLock.WaitAsync(cancellationToken);
        try
        {
            const string commandText = """
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
