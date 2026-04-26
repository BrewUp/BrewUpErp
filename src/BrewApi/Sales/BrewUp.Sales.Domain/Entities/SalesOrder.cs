using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Enums;
using BrewUp.Sales.SharedKernel.Messages.Events;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using Muflone.Core;
using Muflone.CustomTypes;

namespace BrewUp.Sales.Domain.Entities;

public class SalesOrder : AggregateRoot
{
    private SalesOrderNumber _salesOrderNumber = null!;
    private SalesOrderDate _salesOrderDate = null!;
    private Customer _customer = null!;
    private SalesOrderDeliveryDate _salesOrderDeliveryDate = null!;
    private List<SalesOrderRowJson> _rows = [];
    
    protected SalesOrder()
    {}

    internal static SalesOrder Create(SalesOrderId aggregateId, SalesOrderNumber salesOrderNumber,
        SalesOrderDate salesOrderDate, Customer customer, SalesOrderDeliveryDate salesOrderDeliveryDate,
        IEnumerable<SalesOrderRowJson> rows, Guid correlationId)
    {
        return new SalesOrder(aggregateId, salesOrderNumber, salesOrderDate, customer, salesOrderDeliveryDate, rows,
            correlationId);
    }

    private SalesOrder(SalesOrderId aggregateId, SalesOrderNumber salesOrderNumber, SalesOrderDate salesOrderDate,
        Customer customer, SalesOrderDeliveryDate salesOrderDeliveryDate,
        IEnumerable<SalesOrderRowJson> rows, Guid correlationId)
    {
        // Business logic validations can be added here
        if (customer.CustomerType.Equals(CustomerType.Gold))
        {
            // Apply discount
        }
            
        RaiseEvent(new SalesOrderCreated(aggregateId, salesOrderNumber, salesOrderDate, customer,
            salesOrderDeliveryDate, rows, correlationId));
    }

    private void Apply(SalesOrderCreated @event)
    {
        Id = @event.AggregateId;
        _salesOrderNumber = @event.SalesOrderNumber;
        _salesOrderDate = @event.SalesOrderDate;
        _customer = @event.Customer;
        _salesOrderDeliveryDate = @event.SalesOrderDeliveryDate;
        _rows = @event.Rows.ToList();
    }
    
    internal void CloseSalesOrder(SalesOrderDeliveryDate salesOrderDeliveryDate, Account account, Guid correlationId)
    {
        // Business logic validations can be added here
        RaiseEvent(new SalesOrderClosed(new SalesOrderId(Id.Value), salesOrderDeliveryDate, correlationId));
    }

    private void Apply(SalesOrderClosed @event)
    {
        _salesOrderDeliveryDate = @event.SalesOrderDeliveryDate;
    }
}