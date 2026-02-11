namespace BrewApp.Mobile.Services.Api.Dtos;

/// <summary>
/// DTO for StockItem entity from the Warehouse API.
/// Matches the schema from contracts/mobile-app-apis.openapi.yaml.
/// </summary>
public class StockItemDto
{
    public Guid? BeerId { get; set; }
    public string? BeerName { get; set; }
    public string? Location { get; set; }
    public int? QuantityOnHand { get; set; }
    public string? Unit { get; set; }
}
