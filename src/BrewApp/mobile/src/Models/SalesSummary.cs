namespace BrewApp.Mobile.Models;

/// <summary>
/// Internal model for SalesSummary entity.
/// Represents aggregated sales metrics for a given period.
/// </summary>
public class SalesSummary
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int OrdersCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<string> TopSellingBeers { get; set; } = new();
}
