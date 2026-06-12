using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.Core.Chunking;

public class DefaultChunkingPolicy : IChunkingPolicy
{
    public ChunkingOptions GetOptionsFor(KnowledgeDocument document)
    {
        var length = document.Content.Length;

        return length switch
        {
            < 2_000 => new ChunkingOptions(500, 80),
            < 10_000 => new ChunkingOptions(800, 120),
            _ => new ChunkingOptions(1_200, 200)
        };
    }
}