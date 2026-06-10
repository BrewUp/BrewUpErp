using BrewUp.Knowledge.Core.Chunks;

namespace BrewUp.Knowledge.Core.Search;

public sealed record KnowledgeSearchMatch(KnowledgeChunk Chunk, double Score);
