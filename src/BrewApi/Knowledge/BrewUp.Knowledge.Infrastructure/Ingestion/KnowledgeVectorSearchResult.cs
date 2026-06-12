using BrewUp.Knowledge.SharedKernel.Chunks;

namespace BrewUp.Knowledge.Infrastructure.Ingestion;

public sealed record KnowledgeVectorSearchResult(
    KnowledgeChunk Chunk,
    double Score);
