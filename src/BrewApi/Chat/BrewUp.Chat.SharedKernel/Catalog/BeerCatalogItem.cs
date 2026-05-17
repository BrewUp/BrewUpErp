namespace BrewUp.Chat.SharedKernel.Catalog;

public sealed record BeerCatalogItem(
    string BeerId,
    string Name,
    string Style,
    decimal? AlcoholByVolume,
    bool IsActive);
