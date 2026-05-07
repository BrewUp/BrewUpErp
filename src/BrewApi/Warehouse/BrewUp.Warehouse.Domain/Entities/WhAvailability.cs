using BrewUp.Shared.DomainIds;
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
    }
}
