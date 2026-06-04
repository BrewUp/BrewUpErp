using BrewUp.Knowledge.Core.Chunks;
using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Core.Chunking;

internal sealed class SemanticChunkingStrategy : IChunkingStrategy
{
    public IReadOnlyCollection<KnowledgeChunk> Split(KnowledgeDocument document)
    {
        throw new NotImplementedException();
    }
}