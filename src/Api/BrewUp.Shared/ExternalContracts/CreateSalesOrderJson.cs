namespace BrewUp.Shared.ExternalContracts;

public class CreateSalesOrderJson
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    
    public DateTime DeliveryDate { get; set; }
    
    public IEnumerable<SalesOrderRowJson> Rows { get; set; } = [];
}