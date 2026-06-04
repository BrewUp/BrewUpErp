using BrewUp.Knowledge.Core.Documents;

namespace BrewUp.Knowledge.Core.Search;

public sealed class KnowledgeSearchRequest
{
    public string Query { get; init; } = string.Empty;    
    public DocumentScope? Scope { get; init; }
}