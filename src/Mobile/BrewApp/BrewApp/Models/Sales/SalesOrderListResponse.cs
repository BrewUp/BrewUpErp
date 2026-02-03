namespace BrewApp.Models.Sales;

public class SalesOrderListResponse
{
    public List<SalesOrder> Results { get; set; } = new();
    public int PageSize { get; set; }
    public int Page { get; set; }
    public int TotalRecords { get; set; }
}
