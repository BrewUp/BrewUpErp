using BrewUp.Dashboards.Domain;
using BrewUp.Shared.ExternalContracts.Dashboards;
using BrewUp.Shared.ReadModel;
using Lena.Core;
using Microsoft.Extensions.Logging;

namespace BrewUp.Dashboards.Facade;

internal sealed class DashboardsFacade(IDashboardsDomainService dashboardsDomainService,
    ILoggerFactory loggerFactory) : IDashboardsFacade
{
    private readonly ILogger<DashboardsFacade> _logger = loggerFactory.CreateLogger<DashboardsFacade>();

    public Task<Result<PagedResult<SalesForCustomerJson>>> GetSalesByCustomerAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new Result<PagedResult<SalesForCustomerJson>>());
    }
}