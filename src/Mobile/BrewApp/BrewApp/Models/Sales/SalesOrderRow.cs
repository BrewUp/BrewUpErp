namespace BrewApp.Models.Sales;

public class SalesOrderRow
{
    public string BeerId { get; set; } = string.Empty;
    public string BeerName { get; set; } = string.Empty;
    public Quantity Quantity { get; set; } = new();
    public Price Price { get; set; } = new();
}
