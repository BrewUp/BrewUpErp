using System.Data;
using System.Text.Json;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Documents;
using Microsoft.Data.SqlClient;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class SqlServerKnowledgeDocumentRepository(
    SqlServerKnowledgeVectorStoreOptions options) : IKnowledgeDocumentRepository
{
    public async Task StoreAsync(
        KnowledgeDocument document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await SqlServerKnowledgeSchema.EnsureCreatedAsync(
            connection,
            options.Dimensions,
            cancellationToken);

        const string commandText = """
            UPDATE [dbo].[KnowledgeDocuments]
            SET Title = @title,
                DocumentsContent = @documentContent,
                Source = @source,
                Scope = @scope,
                Tags = @tags,
                ImportedAt = @importedAt
            WHERE Id = @id;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO [dbo].[KnowledgeDocuments]
                (
                    Id,
                    Title,
                    DocumentsContent,
                    Source,
                    Scope,
                    Tags,
                    ImportedAt
                )
                VALUES
                (
                    @id,
                    @title,
                    @documentContent,
                    @source,
                    @scope,
                    @tags,
                    @importedAt
                );
            END;
            """;

        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = document.Id;
        command.Parameters.Add("@title", SqlDbType.NVarChar, 300).Value = document.Title;
        command.Parameters.Add("@documentContent", SqlDbType.NVarChar, -1).Value = document.DocumentsContent;
        command.Parameters.Add("@source", SqlDbType.NVarChar, 50).Value = document.Source.Name;
        command.Parameters.Add("@scope", SqlDbType.NVarChar, 50).Value = document.Scope.Name;
        command.Parameters.Add("@tags", SqlDbType.NVarChar, -1).Value =
            JsonSerializer.Serialize(document.Tags);
        command.Parameters.Add("@importedAt", SqlDbType.DateTime2).Value = document.ImportedAt;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<KnowledgeDocument>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await SqlServerKnowledgeSchema.EnsureCreatedAsync(
            connection,
            options.Dimensions,
            cancellationToken);

        const string commandText = """
            SELECT Id, Title, DocumentsContent, Source, Scope, Tags, ImportedAt
            FROM [dbo].[KnowledgeDocuments]
            ORDER BY ImportedAt DESC, Title, Id;
            """;

        await using var command = new SqlCommand(commandText, connection);
        var documents = new List<KnowledgeDocument>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
            documents.Add(ReadDocument(reader));

        return documents;
    }

    public async Task<KnowledgeDocument?> GetByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await SqlServerKnowledgeSchema.EnsureCreatedAsync(
            connection,
            options.Dimensions,
            cancellationToken);

        const string commandText = """
            SELECT Id, Title, DocumentsContent, Source, Scope, Tags, ImportedAt
            FROM [dbo].[KnowledgeDocuments]
            WHERE Id = @documentId;
            """;

        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = documentId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? ReadDocument(reader)
            : null;
    }

    public async Task<bool> DeleteAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await SqlServerKnowledgeSchema.EnsureCreatedAsync(
            connection,
            options.Dimensions,
            cancellationToken);

        const string commandText = """
            DELETE FROM [dbo].[KnowledgeDocuments]
            WHERE Id = @documentId;
            """;

        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = documentId;
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static KnowledgeDocument ReadDocument(SqlDataReader reader)
        => new()
        {
            Id = reader.GetGuid(0),
            Title = reader.GetString(1),
            DocumentsContent = reader.GetString(2),
            Source = Core.Documents.DocumentSource.FromName(reader.GetString(3)),
            Scope = SharedKernel.Enums.DocumentScope.FromName(reader.GetString(4)),
            Tags = JsonSerializer.Deserialize<string[]>(reader.GetString(5)) ?? [],
            ImportedAt = reader.GetDateTime(6)
        };

    private async Task<SqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new InvalidOperationException(
                "A SQL Server connection string is required for Knowledge persistence.");

        var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
