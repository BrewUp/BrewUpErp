using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Core.Search;

public sealed class KnowledgeSearchRequest
{
    public string Query { get; init; } = string.Empty;    
    public DocumentScope? Scope { get; init; }
    public int MaxResults { get; init; } = 5;
    public double MinimumScore { get; init; }
}
