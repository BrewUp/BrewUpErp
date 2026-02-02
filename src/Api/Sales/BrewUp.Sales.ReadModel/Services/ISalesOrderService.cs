using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ReadModel;
using Lena.Core;

namespace BrewUp.Sales.ReadModel.Services;

public interface ISalesOrderService
{
    Task<Result<PagedResult<SalesOrderJson>>> GetSalesOrdersAsync(int page, int pageSize, CancellationToken cancellationToken);
}