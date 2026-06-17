using BrewUp.Shared.ExternalContracts.MasterData.Beers;

namespace BrewUp.Shared.Agents;

public sealed record DemandItem(
    string BeerName,
    decimal Quantity,
    string UnitOfMeasure);

public sealed record ResolvedBeerDemand(
    DemandItem Demand,
    BeerJson Beer);

public sealed record SalesDemandLine(
    string BeerId,
    string BeerName,
    decimal Quantity,
    string UnitOfMeasure,
    decimal UnitPrice,
    decimal LineAmount);

public sealed record SalesDemandSignal(
    IReadOnlyCollection<SalesDemandLine> Lines,
    decimal TotalQuantity,
    decimal EstimatedAmount);

public sealed record WarehouseImpactLine(
    string BeerId,
    string BeerName,
    decimal RequestedQuantity,
    decimal AvailableQuantity,
    decimal RemainingQuantity,
    decimal ReorderThreshold,
    bool StockRisk,
    bool ReorderRisk,
    string UnitOfMeasure);

public sealed record WarehouseImpact(
    IReadOnlyCollection<WarehouseImpactLine> Lines,
    bool HasStockRisk,
    bool HasReorderRisk);

public sealed record KnowledgeFinding(
    string Title,
    string Scope,
    string Content,
    double Score);

public sealed record KnowledgeResult(
    IReadOnlyCollection<KnowledgeFinding> Findings);
