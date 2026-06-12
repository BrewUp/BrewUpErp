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
        await SqlServerKnowledgeSchema.EnsureCreatedAsync(connection, cancellationToken);

        const string commandText = """
            UPDATE [dbo].[KnowledgeDocuments]
            SET Title = @title,
                Content = @content,
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
                    Content,
                    Source,
                    Scope,
                    Tags,
                    ImportedAt
                )
                VALUES
                (
                    @id,
                    @title,
                    @content,
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
        command.Parameters.Add("@content", SqlDbType.NVarChar, -1).Value = document.Content;
        command.Parameters.Add("@source", SqlDbType.NVarChar, 50).Value = document.Source.Name;
        command.Parameters.Add("@scope", SqlDbType.NVarChar, 50).Value = document.Scope.Name;
        command.Parameters.Add("@tags", SqlDbType.NVarChar, -1).Value =
            JsonSerializer.Serialize(document.Tags);
        command.Parameters.Add("@importedAt", SqlDbType.DateTime2).Value = document.ImportedAt;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

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
