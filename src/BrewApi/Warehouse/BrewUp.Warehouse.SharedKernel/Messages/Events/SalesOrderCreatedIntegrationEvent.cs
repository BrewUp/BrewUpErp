using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Warehouse.SharedKernel.CustomTypes;
using Muflone.Messages.Events;

namespace BrewUp.Warehouse.SharedKernel.Messages.Events;

public class SalesOrderCreatedIntegrationEvent(SalesOrderId aggregateId,
    string salesOrderNumber,
    DateTime salesOrderDate,
    string customerId,
    string customerName,
    DateTime salesOrderDeliveryDate,
    IEnumerable<SalesOrderRowJson> rows) : IntegrationEvent(aggregateId)
{
    public string SalesOrderNumber { get; private set; } = salesOrderNumber;
    public DateTime SalesOrderDate { get; private set; } = salesOrderDate;
    
    public string CustomerId { get; private set; } = customerId;
    public string CustomerName { get; private set; } = customerName;
    
    public DateTime SalesOrderDeliveryDate { get; private set; } = salesOrderDeliveryDate;
    public IEnumerable<SalesOrderRowJson> Rows { get; private set; } = rows;
}