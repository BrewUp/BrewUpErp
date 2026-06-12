using System.Data;
using System.Text.Json;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;
using Microsoft.Data.SqlClient;

namespace BrewUp.Knowledge.Infrastructure.Repositories;

public sealed class SqlServerKnowledgeVectorStore(
    SqlServerKnowledgeVectorStoreOptions options) : IKnowledgeVectorStore
{
    private const string TableName = "[dbo].[KnowledgeVectors]";
    private readonly SemaphoreSlim _schemaLock = new(1, 1);
    private bool _schemaInitialized;

    public async Task StoreAsync(
        KnowledgeChunk chunk,
        EmbeddingVector embedding,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(embedding);
        ValidateDimensions(embedding);

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        var commandText = $"""
            UPDATE {TableName}
            SET DocumentId = @documentId,
                Sequence = @sequence,
                Title = @title,
                Scope = @scope,
                Tags = @tags,
                Content = @content,
                TokenCount = @tokenCount,
                MaxCharacters = @maxCharacters,
                OverlapCharacters = @overlapCharacters,
                Embedding = CAST(@embedding AS VECTOR({options.Dimensions}))
            WHERE ChunkId = @chunkId;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO {TableName}
                (
                    ChunkId,
                    DocumentId,
                    Sequence,
                    Title,
                    Scope,
                    Tags,
                    Content,
                    TokenCount,
                    MaxCharacters,
                    OverlapCharacters,
                    Embedding
                )
                VALUES
                (
                    @chunkId,
                    @documentId,
                    @sequence,
                    @title,
                    @scope,
                    @tags,
                    @content,
                    @tokenCount,
                    @maxCharacters,
                    @overlapCharacters,
                    CAST(@embedding AS VECTOR({options.Dimensions}))
                );
            END;
            """;

        await using var command = new SqlCommand(commandText, connection);
        AddChunkParameters(command, chunk);
        command.Parameters.Add("@embedding", SqlDbType.NVarChar, -1).Value =
            JsonSerializer.Serialize(embedding.Values);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<KnowledgeVectorSearchResult>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int topK,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topK);
        ValidateDimensions(queryEmbedding);

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        var commandText = $"""
            SELECT TOP (@topK)
                ChunkId,
                DocumentId,
                Sequence,
                Title,
                Scope,
                Tags,
                Content,
                TokenCount,
                MaxCharacters,
                OverlapCharacters,
                1.0 - VECTOR_DISTANCE(
                    'cosine',
                    CAST(@embedding AS VECTOR({options.Dimensions})),
                    Embedding) AS Score
            FROM {TableName}
            WHERE @scope IS NULL OR Scope = @scope
            ORDER BY Score DESC, Sequence, ChunkId;
            """;

        await using var command = new SqlCommand(commandText, connection);
        command.Parameters.Add("@topK", SqlDbType.Int).Value = topK;
        command.Parameters.Add("@scope", SqlDbType.NVarChar, 50).Value =
            scope is null ? DBNull.Value : scope.Name;
        command.Parameters.Add("@embedding", SqlDbType.NVarChar, -1).Value =
            JsonSerializer.Serialize(queryEmbedding.Values);

        var results = new List<KnowledgeVectorSearchResult>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var chunk = new KnowledgeChunk
            {
                Id = reader.GetGuid(0),
                DocumentId = reader.GetGuid(1),
                Sequence = reader.GetInt32(2),
                Content = reader.GetString(6),
                Metadata = new ChunkMetadata
                {
                    Title = reader.GetString(3),
                    Scope = DocumentScope.FromName(reader.GetString(4)),
                    Tags = JsonSerializer.Deserialize<string[]>(reader.GetString(5)) ?? [],
                    TokenCount = reader.GetInt32(7),
                    MaxCharacters = reader.GetInt32(8),
                    OverlapCharacters = reader.GetInt32(9)
                }
            };

            results.Add(new KnowledgeVectorSearchResult(chunk, reader.GetDouble(10)));
        }

        return results;
    }

    private async Task<SqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new InvalidOperationException(
                "A SQL Server connection string is required for the Knowledge vector store.");

        var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private async Task EnsureSchemaAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        if (_schemaInitialized)
            return;

        await _schemaLock.WaitAsync(cancellationToken);
        try
        {
            if (_schemaInitialized)
                return;

            var commandText = $"""
                IF OBJECT_ID(N'dbo.KnowledgeVectors', N'U') IS NULL
                BEGIN
                    CREATE TABLE {TableName}
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
                        Embedding VECTOR({options.Dimensions}) NOT NULL
                    );

                    CREATE INDEX IX_KnowledgeVectors_DocumentId
                        ON {TableName} (DocumentId, Sequence);

                    CREATE INDEX IX_KnowledgeVectors_Scope
                        ON {TableName} (Scope);
                END;
                """;

            await using var command = new SqlCommand(commandText, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
            _schemaInitialized = true;
        }
        finally
        {
            _schemaLock.Release();
        }
    }

    private void ValidateDimensions(EmbeddingVector embedding)
    {
        if (options.Dimensions is < 1 or > 1998)
            throw new InvalidOperationException(
                "SQL Server vector dimensions must be between 1 and 1998.");

        if (embedding.Dimensions != options.Dimensions)
            throw new InvalidOperationException(
                $"Expected an embedding with {options.Dimensions} dimensions, " +
                $"but received {embedding.Dimensions}.");
    }

    private static void AddChunkParameters(
        SqlCommand command,
        KnowledgeChunk chunk)
    {
        command.Parameters.Add("@chunkId", SqlDbType.UniqueIdentifier).Value = chunk.Id;
        command.Parameters.Add("@documentId", SqlDbType.UniqueIdentifier).Value = chunk.DocumentId;
        command.Parameters.Add("@sequence", SqlDbType.Int).Value = chunk.Sequence;
        command.Parameters.Add("@title", SqlDbType.NVarChar, 500).Value = chunk.Metadata.Title;
        command.Parameters.Add("@scope", SqlDbType.NVarChar, 50).Value = chunk.Metadata.Scope.Name;
        command.Parameters.Add("@tags", SqlDbType.NVarChar, -1).Value =
            JsonSerializer.Serialize(chunk.Metadata.Tags);
        command.Parameters.Add("@content", SqlDbType.NVarChar, -1).Value = chunk.Content;
        command.Parameters.Add("@tokenCount", SqlDbType.Int).Value = chunk.Metadata.TokenCount;
        command.Parameters.Add("@maxCharacters", SqlDbType.Int).Value =
            chunk.Metadata.MaxCharacters;
        command.Parameters.Add("@overlapCharacters", SqlDbType.Int).Value =
            chunk.Metadata.OverlapCharacters;
    }
}
