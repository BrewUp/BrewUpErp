namespace BrewUp.Mother.Contracts;

public sealed record WhatIfInventoryImpactResult(
    MotherAnalysisStatus Status,
    string Summary,
    string? BeerId,
    string? BeerName,
    decimal RequestedQuantity,
    IReadOnlyCollection<InventoryImpactItem> Items,
    string Recommendation);