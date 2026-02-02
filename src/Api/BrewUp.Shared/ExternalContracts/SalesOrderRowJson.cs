namespace BrewUp.Shared.ExternalContracts;

public class SalesOrderRowJson
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    
    public ProductQuantity Quantity { get; init; } = new (0, string.Empty);
    public ProductPrice Price { get; init; } = new(0, string.Empty); 
}