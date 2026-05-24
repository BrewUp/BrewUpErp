using BrewSpa.Sales.Application.Models;
using BrewSpa.Shared.Models;
using Lena.Core;

namespace BrewSpa.Sales.Application.Services;

public interface ISalesService
{
    Task<Result<PagedResult<SalesOrderJson>>> GetSalesOrdersAsync(int page = 1, int pageSize = 10);
    Task<Result<SalesOrderJson>> GetSalesOrderByIdAsync(string orderId);
    Task<Result<SalesOrderJson>> CreateSalesOrderAsync(CreateSalesOrderJson order);
}
