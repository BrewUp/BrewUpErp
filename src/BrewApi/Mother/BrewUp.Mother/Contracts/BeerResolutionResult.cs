namespace BrewUp.Mother.Contracts;

/// <summary>
/// Result returned by the MasterData MCP tool <c>masterdata_resolve_beer</c>.
/// </summary>
public sealed record BeerResolutionResult(
    bool Found,
    string? BeerId,
    string? BeerName);

