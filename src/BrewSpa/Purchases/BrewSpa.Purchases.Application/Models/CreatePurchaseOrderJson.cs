using System.ComponentModel.DataAnnotations;

namespace BrewSpa.Purchases.Application.Models;

public class CreatePurchaseOrderJson
{
    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string SupplierId { get; set; } = string.Empty;

    public DateTime? DeliveryDate { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one order row is required.")]
    public List<PurchaseOrderRowJson> Rows { get; set; } = [];
}
