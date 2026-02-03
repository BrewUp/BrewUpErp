using BrewUp.Sales.Domain;
using BrewUp.Sales.ReadModel.Services;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.ReadModel;
using Lena.Core;

namespace BrewUp.Sales.Facade;

internal class SalesFacade(ISalesDomainService salesDomainService,
    ISalesOrderService salesOrderService) : ISalesFacade
{
    public Task<Result<string>> CreateSalesOrderAsync(CreateSalesOrderJson body, CancellationToken cancellationToken) =>
        salesDomainService.CreateSalesOrderAsync(body, cancellationToken);

    public Task<Result<PagedResult<SalesOrderJson>>> GetSalesOrdersAsync(int page, int pageSize,
        CancellationToken cancellationToken) =>
        salesOrderService.GetSalesOrdersAsync(page, pageSize, cancellationToken);

    public Task<Result<SalesOrderJson>> GetSalesOrderByIdAsync(string orderId, CancellationToken cancellationToken) =>
        salesOrderService.GetSalesOrderByIdAsync(orderId, cancellationToken);
}