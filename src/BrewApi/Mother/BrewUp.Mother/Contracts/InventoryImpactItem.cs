namespace BrewUp.Mother.Contracts;

public sealed record InventoryImpactItem(
    string BeerId,
    string BeerName,
    decimal RequestedQuantity,
    decimal AvailableQuantity,
    decimal ResidualQuantity,
    decimal ReorderThreshold,
    bool BelowReorderThreshold,
    string Reason);