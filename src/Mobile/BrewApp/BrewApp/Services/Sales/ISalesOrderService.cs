using BrewApp.Models.Sales;

namespace BrewApp.Services.Sales;

public interface ISalesOrderService
{
    Task<SalesOrderListResponse> GetSalesOrdersAsync(int page = 0, int pageSize = 10);
    Task<SalesOrder?> GetSalesOrderByIdAsync(string id);
    Task<SalesOrder?> CreateSalesOrderAsync(CreateSalesOrderRequest request);
}
