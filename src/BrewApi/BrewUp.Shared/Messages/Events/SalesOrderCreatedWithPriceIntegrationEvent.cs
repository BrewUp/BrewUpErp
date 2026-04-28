using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using Muflone.Messages.Events;

namespace BrewUp.Shared.Messages.Events;

public class SalesOrderCreatedWihPriceIntegrationEvent(SalesOrderId aggregateId,
    string salesOrderNumber,
    DateTime salesOrderDate,
    string customerId,
    string customerName,
    DateTime salesOrderDeliveryDate,
    IEnumerable<OrderRowWithPriceDto> rows) : IntegrationEvent(aggregateId)
{
    public string SalesOrderNumber { get; private set; } = salesOrderNumber;
    public DateTime SalesOrderDate { get; private set; } = salesOrderDate;
    
    public string CustomerId { get; private set; } = customerId;
    public string CustomerName { get; private set; } = customerName;
    
    public DateTime SalesOrderDeliveryDate { get; private set; } = salesOrderDeliveryDate;
    public IEnumerable<OrderRowWithPriceDto> Rows { get; private set; } = rows;
}