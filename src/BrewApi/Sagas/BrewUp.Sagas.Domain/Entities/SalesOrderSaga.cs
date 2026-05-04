using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.Messages.Events.Sagas;
using Muflone.Core;

namespace BrewUp.Sagas.Domain.Entities;

public class SalesOrderSaga : AggregateRoot
{
    private string _salesOrderNumber = string.Empty;
    private DateTime _salesOrderDate;
    private string _customerId = string.Empty;
    private DateTime _salesOrderDeliveryDate;
    private List<SalesOrderRowJson> _rows = [];
    
    private DateTime _startDate;
    private DateTime _endDate;
    
    protected SalesOrderSaga()
    {}

    internal static SalesOrderSaga Create(IntegrationId aggregateId, Guid correlationId, string salesOrderNumber,
        DateTime salesOrderDate, string customerId, DateTime salesOrderDeliveryDate,
        IEnumerable<SalesOrderRowJson> rows)
    {
        return new SalesOrderSaga(aggregateId, correlationId, salesOrderNumber, salesOrderDate, customerId,
            salesOrderDeliveryDate, rows);
    }

    private SalesOrderSaga(IntegrationId aggregateId, Guid correlationId, string salesOrderNumber,
        DateTime salesOrderDate, string customerId, DateTime salesOrderDeliveryDate,
        IEnumerable<SalesOrderRowJson> rows)
    {
        RaiseEvent(new SalesOrderPlaced(aggregateId, correlationId, salesOrderNumber, salesOrderDate, customerId,
            salesOrderDeliveryDate, rows));
    }

    private void Apply(SalesOrderPlaced @event)
    {
        Id = @event.AggregateId;
        _salesOrderNumber = @event.SalesOrderNumber;
        _salesOrderDate = @event.SalesOrderDate;
        _customerId = @event.CustomerId;
        _salesOrderDeliveryDate = @event.SalesOrderDeliveryDate;
        _rows = @event.Rows.ToList();
        
        _startDate = DateTime.UtcNow;        
    }
}