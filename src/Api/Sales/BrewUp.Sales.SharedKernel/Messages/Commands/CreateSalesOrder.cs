using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using Muflone.Messages.Commands;

namespace BrewUp.Sales.SharedKernel.Messages.Commands;

public sealed class CreateSalesOrder(
    SalesOrderId aggregateId,
    SalesOrderNumber salesOrderNumber,
    SalesOrderDate salesOrderDate,
    CustomerId customerId,
    CustomerName customerName,
    SalesOrderDeliveryDate salesOrderDeliveryDate,
    IEnumerable<SalesOrderRowJson> rows,
    Guid correlationId) : Command(aggregateId, correlationId)
{
    public SalesOrderNumber SalesOrderNumber { get; private set; } = salesOrderNumber;
    public SalesOrderDate SalesOrderDate { get; private set; } = salesOrderDate;
    
    public CustomerId CustomerId { get; private set; } = customerId;
    public CustomerName CustomerName { get; private set; } = customerName;
    
    public SalesOrderDeliveryDate SalesOrderDeliveryDate { get; private set; } = salesOrderDeliveryDate;
    public IEnumerable<SalesOrderRowJson> Rows { get; private set; } = rows;
}