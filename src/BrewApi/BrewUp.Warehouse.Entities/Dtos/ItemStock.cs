using BrewUp.Shared.ExternalContracts.Warehouse;
using System.ComponentModel.DataAnnotations;

namespace BrewUp.Warehouse.Entities.Dtos
{
    public class ItemStock
    {
        protected ItemStock() { }

        internal ItemStock(string beerId, string quantity) 
        {
            BeerId = beerId;
            Quantity = quantity;
        }

        public string BeerId { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;

        public ItemStockJson ToJson()
        {
            return new ItemStockJson
            {
                BeerId = BeerId,
                Quantity = Quantity
            };
        }
    }
}