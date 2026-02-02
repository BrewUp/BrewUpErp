using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using BrewUp.Shared.ExternalContracts;
using Lena.Core;
using Muflone.Persistence;

namespace BrewUp.Sales.Domain;

internal class SalesDomainService(IServiceBus serviceBus) : ISalesDomainService
{
    public async Task<Result<string>> CreateSalesOrderAsync(CreateSalesOrderJson body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var salesOrderId = Guid.NewGuid().ToString();
        
        CreateSalesOrder command = new CreateSalesOrder(new SalesOrderId(salesOrderId),
            new SalesOrderNumber(body.OrderNumber),
            new SalesOrderDate(body.OrderDate),
            new CustomerId(body.CustomerId),
            new CustomerName(body.CustomerName),
            new SalesOrderDeliveryDate(body.DeliveryDate),
            body.Rows, Guid.NewGuid());

        await serviceBus.SendAsync(command, cancellationToken);
        
        return Result<string>.Success(salesOrderId);
    }
}