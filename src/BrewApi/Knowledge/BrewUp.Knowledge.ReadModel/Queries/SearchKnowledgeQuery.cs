namespace BrewUp.Knowledge.ReadModel.Queries;

public sealed record SearchKnowledgeQuery(
    string Query,
    string? Scope = null,
    int? TopK = null);