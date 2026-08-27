namespace BrewUp.Knowledge.ReadModel.Queries;

public sealed record QueryWiki(
    string Query,
    string? Scope = null,
    int? TopK = null);

