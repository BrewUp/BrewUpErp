namespace BrewUp.Mother.Contracts;

/// <summary>
/// Result returned by the Warehouse MCP tool <c>warehouse_get_item_availability</c>.
/// </summary>
public sealed record WarehouseAvailability(
    string BeerId,
    decimal AvailableQuantity,
    decimal ReorderThreshold);

