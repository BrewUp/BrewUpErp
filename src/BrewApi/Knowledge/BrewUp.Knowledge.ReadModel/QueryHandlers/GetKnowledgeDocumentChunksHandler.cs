using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.ReadModel.Queries;

namespace BrewUp.Knowledge.ReadModel.QueryHandlers;

public sealed class GetKnowledgeDocumentChunksHandler(
    IKnowledgeChunkRepository chunkRepository)
{
    public async Task<GetKnowledgeDocumentChunksResult> HandleAsync(
        GetKnowledgeDocumentChunksQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var chunks = await chunkRepository.GetByDocumentIdAsync(
            query.DocumentId,
            cancellationToken);

        var resultChunks = chunks
            .OrderBy(chunk => chunk.Sequence)
            .Select(chunk => new KnowledgeDocumentChunkResult(
                chunk.Id,
                chunk.Sequence,
                chunk.Metadata.TokenCount,
                chunk.KnowledgeContent))
            .ToArray();

        return new GetKnowledgeDocumentChunksResult(
            query.DocumentId,
            resultChunks.Length,
            resultChunks);
    }
}
