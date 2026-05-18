namespace BrewSpa.Sales.Application.Models;

public class SalesOrderJson
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public List<SalesOrderRowJson> Rows { get; set; } = [];
    public string Status { get; set; } = string.Empty;
}
