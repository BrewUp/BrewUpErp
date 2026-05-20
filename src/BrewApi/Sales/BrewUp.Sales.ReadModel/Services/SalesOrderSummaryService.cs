using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Enums;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ReadModel;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SalesOrderSummary = BrewUp.Sales.ReadModel.Dtos.SalesOrderSummary;

namespace BrewUp.Sales.ReadModel.Services;

internal sealed class SalesOrderSummaryService([FromKeyedServices("sales")] IPersister persister,
    IQueries<SalesOrderSummary> orderSummaryQueries,
    ILoggerFactory loggerFactory) : ServiceBase(persister, loggerFactory), ISalesOrderSummaryService
{
    public async Task<Result<bool>> CreateSalesOrderAsync(SalesOrderId salesOrderId, SalesOrderNumber salesOrderNumber, CustomerId customerId,
        CustomerName customerName, SalesOrderDate orderDate, Price totalAmount, SalesOrderStatus status,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var salesOrder =
            SalesOrderSummary.Create(salesOrderId, salesOrderNumber, customerId, customerName, orderDate,
                totalAmount, status);
        var insertResult = await Persister.InsertAsync(salesOrder, cancellationToken);

        return insertResult.Match(
            _ => Result<bool>.Success(true),
            error =>
            {
                Logger.LogError("Error creating sales order sumary: {Error}", error);
                return Result<bool>.Error($"Error creating sales order summary: {error}");
            });
    }
}