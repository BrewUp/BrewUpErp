namespace BrewUp.Knowledge.SharedKernel.Documents;

public sealed record KnowledgeSearchResultItem(
    Guid DocumentId,
    Guid ChunkId,
    int Sequence,
    string Title,
    string Scope,
    IReadOnlyCollection<string> Tags,
    string Content,
    double Score,
    int TokenCount);