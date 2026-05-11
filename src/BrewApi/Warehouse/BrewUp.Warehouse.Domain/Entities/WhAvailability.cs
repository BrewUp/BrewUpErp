using CustomTypes = BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Warehouse.Entities.Dtos;
using BrewUp.Warehouse.SharedKernel.Messages.Events;
using Muflone.Core;

namespace BrewUp.Warehouse.Domain.Entities
{
    public class WhAvailability : AggregateRoot
    {
        internal WarehouseId _warehouseId;
        internal BeerId _beerId;
        internal Quantity _quantity;

        protected WhAvailability() { }

        internal static WhAvailability Create(AvailabilityId aggregateId,
            WarehouseId warehouseId,
            BeerId beerId,
            Quantity quantity)
        {
            return new WhAvailability(aggregateId, warehouseId, beerId, quantity);
        }

        private WhAvailability(AvailabilityId aggregateId,
            WarehouseId warehouseId,
            BeerId beerId,
            Quantity quantity)
        {
            RaiseEvent(new WhAvailabilityCreated(aggregateId,
                warehouseId,
                beerId,
                new Shared.CustomTypes.Quantity(quantity.Value, quantity.UnitOfMeasure)));
        }

        private void Apply(WhAvailabilityCreated @event)
        {
            Id = @event.AggregateId;
            _warehouseId = @event.WarehouseId;
            _beerId = @event.BeerId;
            _quantity = new Quantity(@event.Quantity.Value, @event.Quantity.UnitOfMeasure);
        }

        internal void AddItemStock(Quantity quantity)
        {
            if (quantity == null) throw new ArgumentNullException(nameof(quantity));

            if (_quantity == null)
            {
                _quantity = new Quantity(quantity.Value, quantity.UnitOfMeasure);
            }
            else
            {
                //TODO: consider implementing unit conversion if units of measure differ
                if (quantity.UnitOfMeasure != _quantity.UnitOfMeasure) throw new ArgumentException("Unit of measure mismatch", nameof(quantity));

                _quantity = new Quantity(_quantity.Value + quantity.Value, _quantity.UnitOfMeasure);
            }

            RaiseEvent(new ItemStockAdded(new AvailabilityId(Id.Value), new CustomTypes.Quantity(_quantity.Value, _quantity.UnitOfMeasure)));
        }

        private void Apply(ItemStockAdded @event)
        {
            // This replaces the quantity instead of adding it.
            _quantity = new Quantity(@event.Quantity.Value, @event.Quantity.UnitOfMeasure);
        }
    }
}