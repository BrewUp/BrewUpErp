using System.ComponentModel.DataAnnotations;

namespace BrewSpa.Sales.Application.Models;

public class CreateSalesOrderJson
{
    [Required]
    public string OrderNumber { get; set; } = string.Empty;
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    
    [Required]
    public string CustomerName { get; set; } = string.Empty;
    
    public DateTime DeliveryDate { get; set; }
    
    [Required]
    public List<SalesOrderRowJson> Rows { get; set; } = [];
}
