using System.Data;
using System.Text.Json;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Enums;
using Microsoft.Data.SqlClient;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class SqlServerKnowledgeChunkRepository(
    SqlServerKnowledgeVectorStoreOptions options) :
    IKnowledgeChunkRepository,
    IKnowledgeChunkWriter
{
    public async Task StoreAsync(
        IReadOnlyCollection<KnowledgeChunk> chunks,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        if (chunks.Count == 0)
            return;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await SqlServerKnowledgeSchema.EnsureCreatedAsync(connection, cancellationToken);
        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var chunk in chunks)
                await StoreChunkAsync(connection, transaction, chunk, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<KnowledgeChunk>> GetByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await SqlServerKnowledgeSchema.EnsureCreatedAsync(connection, cancellationToken);

        const string commandText = """
            SELECT
                Id,
                DocumentId,
                Sequence,
                Content,
                TokenCount,
                Scope,
                Title,
                Tags
            FROM [dbo].[KnowledgeChunks]
            WHERE DocumentId = @documentId
            ORDER BY Sequence, Id;
            """;

        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = documentId;

        var chunks = new List<KnowledgeChunk>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            chunks.Add(new KnowledgeChunk
            {
                Id = reader.GetGuid(0),
                DocumentId = reader.GetGuid(1),
                Sequence = reader.GetInt32(2),
                Content = reader.GetString(3),
                Metadata = new ChunkMetadata
                {
                    TokenCount = reader.GetInt32(4),
                    Scope = DocumentScope.FromName(reader.GetString(5)),
                    Title = reader.GetString(6),
                    Tags = JsonSerializer.Deserialize<string[]>(reader.GetString(7)) ?? []
                }
            });
        }

        return chunks;
    }

    private static async Task StoreChunkAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        KnowledgeChunk chunk,
        CancellationToken cancellationToken)
    {
        const string commandText = """
            UPDATE [dbo].[KnowledgeChunks]
            SET DocumentId = @documentId,
                Sequence = @sequence,
                Content = @content,
                TokenCount = @tokenCount,
                Scope = @scope,
                Title = @title,
                Tags = @tags
            WHERE Id = @id;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO [dbo].[KnowledgeChunks]
                (
                    Id,
                    DocumentId,
                    Sequence,
                    Content,
                    TokenCount,
                    Scope,
                    Title,
                    Tags
                )
                VALUES
                (
                    @id,
                    @documentId,
                    @sequence,
                    @content,
                    @tokenCount,
                    @scope,
                    @title,
                    @tags
                );
            END;
            """;

        await using var command = new SqlCommand(commandText, connection, transaction);
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = chunk.Id;
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value =
            chunk.DocumentId;
        command.Parameters.Add("@sequence", SqlDbType.Int).Value = chunk.Sequence;
        command.Parameters.Add("@content", SqlDbType.NVarChar, -1).Value = chunk.Content;
        command.Parameters.Add("@tokenCount", SqlDbType.Int).Value =
            chunk.Metadata.TokenCount;
        command.Parameters.Add("@scope", SqlDbType.NVarChar, 50).Value =
            chunk.Metadata.Scope.Name;
        command.Parameters.Add("@title", SqlDbType.NVarChar, 300).Value =
            chunk.Metadata.Title;
        command.Parameters.Add("@tags", SqlDbType.NVarChar, -1).Value =
            JsonSerializer.Serialize(chunk.Metadata.Tags);

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
