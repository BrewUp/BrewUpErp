using System.ComponentModel.DataAnnotations;

namespace BrewUp.Warehouse.Entities.Dtos
{
    public class ItemStockJson
    {
        [Required]
        public string BeerId { get; set; } = string.Empty;
        [Required]
        public string Quantity { get; set; } = string.Empty;
    }
}