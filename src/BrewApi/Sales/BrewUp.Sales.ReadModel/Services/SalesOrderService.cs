using BrewUp.Sales.ReadModel.Dtos;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.ReadModel;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.Services;

internal sealed class SalesOrderService([FromKeyedServices("sales")] IPersister persister,
    IQueries<SalesOrder> orderQueries,
    ILoggerFactory loggerFactory) : ServiceBase(persister, loggerFactory), ISalesOrderService
{
    public async Task<Result<bool>> CreateSalesOrderAsync(SalesOrderId salesOrderId, SalesOrderNumber salesOrderNumber, CustomerId customerId,
        CustomerName customerName, SalesOrderDate orderDate, IEnumerable<SalesOrderRowJson> rows, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var salesOrder = SalesOrder.CreateSalesOrder(salesOrderId, salesOrderNumber, customerId, customerName, orderDate, rows);
        var insertResult = await Persister.InsertAsync(salesOrder, cancellationToken);

        return insertResult.Match(
            _ => Result<bool>.Success(true),
            error =>
            {
                Logger.LogError("Error creating sales order: {Error}", error);
                return Result<bool>.Error($"Error creating sales order: {error}");
            });
    }

    public async Task<Result<PagedResult<SalesOrderJson>>> GetSalesOrdersAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var queryResult = await orderQueries.GetByFilterAsync(null, page, pageSize, cancellationToken);
        
        return queryResult.Match(
            _ =>
            {
                queryResult.TryGetValue(out PagedResult<SalesOrder> pagedResult);
                
                return pagedResult.TotalRecords > 0
                    ? Result<PagedResult<SalesOrderJson>>.Success(new PagedResult<SalesOrderJson>(
                        pagedResult.Results.Select(r => r.ToJson()), 
                        pagedResult.Page, 
                        pagedResult.PageSize, 
                        pagedResult.TotalRecords))
                    : Result<PagedResult<SalesOrderJson>>.Success(new PagedResult<SalesOrderJson>([], 0, 0, 0));
            },
            _ => Result<PagedResult<SalesOrderJson>>.Error("Error retrieving sales orders"));
    }

    public async Task<Result<SalesOrderJson>> GetSalesOrderByIdAsync(string salesOrderId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var queryResult = await orderQueries.GetByIdAsync(salesOrderId, cancellationToken);
        
        return queryResult.Match(
            _ =>
            {
                queryResult.TryGetValue(out SalesOrder salesOrder);
                return Result<SalesOrderJson>.Success(salesOrder.ToJson());
            },
            _ => Result<SalesOrderJson>.Error("Error retrieving sales order"));
    }
}