namespace BrewUp.Shared.ExternalContracts.Warehouse;

public class WhAvailabilityJson
{
    public string Id { get; set; } = null!;
    public string WarehouseId { get; set; } = null!;
    public string BeerId { get; set; } = null!;
    public decimal Quantity { get; set; } = 0;
    public string UnitOfMeasure { get; set; } = null!;
}