namespace BrewUp.Mother.Contracts;

public sealed record WhatIfInventoryImpactRequest(
    string BeerReference,
    decimal Quantity,
    string? OriginalQuestion = null);