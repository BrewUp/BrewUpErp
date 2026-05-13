using BrewUp.Sagas.SharedKernel.CustomTypes;
using BrewUp.Sagas.SharedKernel.Enums;
using BrewUp.Sagas.SharedKernel.Messages.Events;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.MasterData.Customers;
using BrewUp.Shared.ExternalContracts.Sales;
using Muflone.Core;

namespace BrewUp.Sagas.Domain.Entities;

public class SalesOrderSaga : AggregateRoot
{
    private string _salesOrderNumber = string.Empty;
    private DateTime _salesOrderDate;
    private string _customerId = string.Empty;
    private string _warehouseId = string.Empty;
    private CustomerJson _customer;
    private DateTime _salesOrderDeliveryDate;
    private List<SalesOrderRowJson> _rows = [];
    
    private SagaState _status;
    
    private DateTime _startDate;
    private DateTime _endDate;
    
    protected SalesOrderSaga()
    {}

    internal static SalesOrderSaga Start(SagaId aggregateId, Guid correlationId, string salesOrderNumber,
        DateTime salesOrderDate, string customerId, string warehouseId, DateTime salesOrderDeliveryDate,
        IEnumerable<SalesOrderRowJson> rows)
    {
        return new SalesOrderSaga(aggregateId, correlationId, salesOrderNumber, salesOrderDate, customerId, 
            warehouseId, salesOrderDeliveryDate, rows);
    }

    private SalesOrderSaga(SagaId aggregateId, Guid correlationId, string salesOrderNumber,
        DateTime salesOrderDate, string customerId, string warehouseId, DateTime salesOrderDeliveryDate,
        IEnumerable<SalesOrderRowJson> rows)
    {
        RaiseEvent(new SalesOrderSagaStarted(aggregateId, correlationId, salesOrderNumber, salesOrderDate, customerId,
            warehouseId, salesOrderDeliveryDate, rows));
    }

    private void Apply(SalesOrderSagaStarted @event)
    {
        Id = @event.AggregateId;
        _salesOrderNumber = @event.SalesOrderNumber;
        _salesOrderDate = @event.SalesOrderDate;
        _customerId = @event.CustomerId;
        _warehouseId = @event.WarehouseId;
        _salesOrderDeliveryDate = @event.SalesOrderDeliveryDate;
        _rows = @event.Rows.ToList();
        
        _startDate = DateTime.UtcNow;
        _status = SagaState.Accepted;
    }

    internal void MarkAsRejected(string message, Guid correlationId)
    {
        RaiseEvent(new SalesOrderSagaRejected(new IntegrationId(Id.Value), correlationId, message));
    }

    private void Apply(SalesOrderSagaRejected @event)
    {
        _endDate = DateTime.UtcNow;
        _status = SagaState.Rejected;
    }

    internal void MarkCustomerBudgetAsVerified(CustomerJson customer, Guid correlationId)
    {
        RaiseEvent(new SagaCustomerBudgetVerified(new CustomerId(_customerId), correlationId, 
            customer,
            new CreateSalesOrderJson
            {
                OrderNumber = _salesOrderNumber,
                OrderDate = _salesOrderDate,
                CustomerId = _customerId,
                DeliveryDate = _salesOrderDeliveryDate,
                Rows = _rows.ToList()
            }));
    }

    internal void MarkOrderAvailable(Guid correlationId)
    {
        RaiseEvent(new SagaOrderRequestAccepted(new WarehouseId(_warehouseId), correlationId));
    }

    internal void MarkOrderNotAvailable(string message, Guid correlationId)
    {
        RaiseEvent(new SagaOrderRequestRejected(new WarehouseId(_warehouseId), correlationId, message));
    }

    private void Apply(SagaCustomerBudgetVerified @event)
    {
        _customer = @event.Customer;
    }

    internal void MarkSalesOrderAsPlaced(Guid correlationId)
    {
        
    }
}