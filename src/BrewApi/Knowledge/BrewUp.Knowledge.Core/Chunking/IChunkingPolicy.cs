using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.Core.Chunking;

public interface IChunkingPolicy
{
    ChunkingOptions GetOptionsFor(KnowledgeDocument document);
}