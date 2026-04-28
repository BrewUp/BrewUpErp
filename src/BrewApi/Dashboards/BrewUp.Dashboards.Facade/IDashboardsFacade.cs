using BrewUp.Shared.ExternalContracts.Dashboards;
using BrewUp.Shared.ReadModel;
using Lena.Core;

namespace BrewUp.Dashboards.Facade;

public interface IDashboardsFacade
{
    Task<Result<PagedResult<SalesForCustomerJson>>> GetSalesByCustomerAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}