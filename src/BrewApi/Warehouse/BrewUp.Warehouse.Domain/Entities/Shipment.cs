using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Warehouse.SharedKernel.CustomTypes;
using BrewUp.Warehouse.SharedKernel.Enums;
using BrewUp.Warehouse.SharedKernel.Messages.Events;
using Muflone.Core;

namespace BrewUp.Warehouse.Domain.Entities;

public class Shipment : AggregateRoot
{
    private SalesOrderId _salesOrderId;
    private CustomerId _customerId;
    private DeliveryDate _deliveryDate;
    
    private IEnumerable<OrderRowDto> _rows;
    
    private    ShipmentState _shipmentState;
    
    protected Shipment() { }

    internal static Shipment Create(ShipmentId shipmentId, SalesOrderId salesOrderId, CustomerId customerId,
        DeliveryDate deliveryDate,
        IEnumerable<OrderRowDto> rows, Guid correlationId)
    {
        return new Shipment(shipmentId, salesOrderId, customerId, deliveryDate, rows, correlationId);
    }

    private Shipment(ShipmentId shipmentId, SalesOrderId salesOrderId, CustomerId customerId, DeliveryDate deliveryDate,
        IEnumerable<OrderRowDto> rows, Guid correlationId)
    {
        RaiseEvent(new ShipmentReadyForDispatch(shipmentId, salesOrderId, customerId, deliveryDate, rows, correlationId));
    }
    
    private void Apply(ShipmentReadyForDispatch @event)
    {
        Id = @event.AggregateId;
        _salesOrderId = @event.SalesOrderId;
        _customerId = @event.CustomerId;
        _deliveryDate = @event.ShipmentDeliveryDate;
        _rows = @event.Rows;
        _shipmentState = ShipmentState.PendingPreparation;
    }
        
}