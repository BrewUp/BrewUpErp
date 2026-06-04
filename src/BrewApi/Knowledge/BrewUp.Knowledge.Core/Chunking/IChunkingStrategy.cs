using BrewUp.Knowledge.Core.Chunks;
using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Core.Chunking;

public interface IChunkingStrategy
{
    IReadOnlyCollection<KnowledgeChunk> Split(KnowledgeDocument document);
}