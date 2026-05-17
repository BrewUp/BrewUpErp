namespace BrewUp.Mother.CustomTypes;

public record StockRiskDetected(
    string SalesOrderId, 
    string BeerId,
    string BeerName,
    decimal RequiredQuantity,
    decimal AvailableQuantity,
    decimal ReorderThreshold,
    string Reason);