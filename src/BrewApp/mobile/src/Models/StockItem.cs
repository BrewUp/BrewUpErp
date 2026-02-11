namespace BrewApp.Mobile.Models;

/// <summary>
/// Internal model for StockItem entity.
/// Represents inventory/stock level for a beer at a specific location.
/// </summary>
public class StockItem
{
    public Guid BeerId { get; set; }
    public string BeerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public string Unit { get; set; } = string.Empty;
}
