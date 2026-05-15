namespace BrewUp.Mcp.SharedKernel.Catalog;

public sealed record BeerCatalogItem(
    string BeerId,
    string Name,
    string Style,
    decimal? AlcoholByVolume,
    bool IsActive);
