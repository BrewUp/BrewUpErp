using System.ComponentModel.DataAnnotations;

namespace BrewUp.Shared.ExternalContracts.Warehouse;

public class ItemStockJson
{
    [Required]
    public string BeerId { get; set; } = string.Empty;
    [Required]
    public string Quantity { get; set; } = string.Empty;
}
