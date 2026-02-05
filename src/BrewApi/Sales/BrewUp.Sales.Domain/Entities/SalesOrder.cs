using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Events;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using Muflone.Core;

namespace BrewUp.Sales.Domain.Entities;

public class SalesOrder : AggregateRoot
{
    private SalesOrderNumber _salesOrderNumber = null!;
    private SalesOrderDate _salesOrderDate = null!;
    private CustomerId _customerId = null!;
    private CustomerName _customerName = null!;
    private SalesOrderDeliveryDate _salesOrderDeliveryDate = null!;
    private List<SalesOrderRowJson> _rows = [];
    
    protected SalesOrder()
    {}

    internal static SalesOrder Create(SalesOrderId aggregateId, SalesOrderNumber salesOrderNumber,
        SalesOrderDate salesOrderDate,
        CustomerId customerId, CustomerName customerName, SalesOrderDeliveryDate salesOrderDeliveryDate,
        IEnumerable<SalesOrderRowJson> rows, Guid correlationId)
    {
        return new SalesOrder(aggregateId, salesOrderNumber, salesOrderDate, customerId, customerName,
            salesOrderDeliveryDate, rows, correlationId);
    }

    private SalesOrder(SalesOrderId aggregateId, SalesOrderNumber salesOrderNumber, SalesOrderDate salesOrderDate,
        CustomerId customerId, CustomerName customerName, SalesOrderDeliveryDate salesOrderDeliveryDate,
        IEnumerable<SalesOrderRowJson> rows, Guid correlationId)
    {
        // Business logic validations can be added here
        RaiseEvent(new SalesOrderCreated(aggregateId, salesOrderNumber, salesOrderDate, customerId, customerName,
            salesOrderDeliveryDate, rows, correlationId));
    }

    private void Apply(SalesOrderCreated @event)
    {
        Id = @event.AggregateId;
        _salesOrderNumber = @event.SalesOrderNumber;
        _salesOrderDate = @event.SalesOrderDate;
        _customerId = @event.CustomerId;
        _customerName = @event.CustomerName;
        _salesOrderDeliveryDate = @event.SalesOrderDeliveryDate;
        _rows = @event.Rows.ToList();
    }
}