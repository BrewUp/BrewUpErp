namespace BrewApp.Mobile.Services.Api.Dtos;

/// <summary>
/// DTO for SalesSummary entity from the Sales API.
/// Matches the schema from contracts/mobile-app-apis.openapi.yaml.
/// </summary>
public class SalesSummaryDto
{
    public string? Period { get; set; }
    public decimal? TotalRevenue { get; set; }
    public int? OrdersCount { get; set; }
    public decimal? AverageOrderValue { get; set; }
    public List<string>? TopSellingBeers { get; set; }
}
