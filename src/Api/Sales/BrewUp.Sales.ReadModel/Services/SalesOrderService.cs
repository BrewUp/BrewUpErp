using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ReadModel;
using Lena.Core;

namespace BrewUp.Sales.ReadModel.Services;

internal sealed class SalesOrderService : ISalesOrderService
{
    public Task<Result<PagedResult<SalesOrderJson>>> GetSalesOrdersAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new Result<PagedResult<SalesOrderJson>>());
    }
}