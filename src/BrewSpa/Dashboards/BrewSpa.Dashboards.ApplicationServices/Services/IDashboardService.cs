using BrewSpa.Dashboards.ApplicationServices.Models;
using BrewSpa.Shared.Models;
using Lena.Core;

namespace BrewSpa.Dashboards.ApplicationServices.Services;

public interface IDashboardService
{
    Task<Result<PagedResult<SalesByCustomerJson>>> GetSalesByCustomerAsync(int pageNumber = 1, int pageSize = 10);
    Task<Result<PagedResult<SalesByProductJson>>> GetSalesByProductAsync(int pageNumber = 1, int pageSize = 10);
}
