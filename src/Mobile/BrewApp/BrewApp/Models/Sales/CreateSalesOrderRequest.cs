namespace BrewApp.Models.Sales;

public class CreateSalesOrderRequest
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public List<SalesOrderRow> Rows { get; set; } = new();
}
