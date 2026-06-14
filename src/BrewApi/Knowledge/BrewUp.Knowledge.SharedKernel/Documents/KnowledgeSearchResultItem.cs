using System.Text.Json.Serialization;

namespace BrewUp.Knowledge.SharedKernel.Documents;

public sealed record KnowledgeSearchResultItem(
    Guid DocumentId,
    Guid ChunkId,
    int ChunkSequence,
    string DocumentTitle,
    string Scope,
    IReadOnlyCollection<string> Tags,
    string Content,
    double Score,
    int TokenCount)
{
    [JsonIgnore]
    public int Sequence => ChunkSequence;

    [JsonIgnore]
    public string Title => DocumentTitle;
}
