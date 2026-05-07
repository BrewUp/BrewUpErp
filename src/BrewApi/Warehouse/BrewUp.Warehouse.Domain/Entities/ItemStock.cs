using BrewUp.Shared.DomainIds;
using Muflone.Core;

namespace BrewUp.Warehouse.Domain.Entities
{
    public class ItemStock : Entity
    {
        internal BeerId BeerId { get; }
        internal decimal Quantity { get; private set; }   //TODO: Change to Quantity custom type

        protected ItemStock() { }

        internal static ItemStock Create(BeerId beerId, decimal quantity)
        {
            return new ItemStock(beerId, quantity);
        }

        private ItemStock(BeerId beerId, decimal quantity)
        {
            BeerId = beerId;
            Quantity = quantity;
        }

        public bool HasEnoughItems(decimal requiredQuantity)
        {
            return Quantity >= requiredQuantity;
        }

        public void RemoveItems(decimal quantity)
        {
            if (quantity > Quantity)
            {
                throw new InvalidOperationException("Not enough items in stock.");
            }

            Quantity -= quantity;
        }

        public bool TryRemoveItems(decimal quantity)
        {
            if (quantity > Quantity)
            {
                return false;
            }

            Quantity -= quantity;

            return true;
        }
    }
}
