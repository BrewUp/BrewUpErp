using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.Core.Chunking;

public interface IChunkingStrategy
{
    IReadOnlyCollection<KnowledgeChunk> Split(KnowledgeDocument document);
}