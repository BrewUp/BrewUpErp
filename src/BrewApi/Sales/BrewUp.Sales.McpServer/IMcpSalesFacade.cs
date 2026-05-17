using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Sales.McpServer;

public interface IMcpSalesFacade
{
    Task<IReadOnlyCollection<SalesOrderSummary>> GetOpenOrdersAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SalesOrderSummary>> GetOrdersByCustomerAsync(
        string customerName,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SalesOrderSummary>> GetLateOrdersAsync(
        DateOnly businessDate,
        CancellationToken cancellationToken);
    
    Task<CustomerTotalPurchased> GetCustomerTotalPurchasedAsync(
        string customerId,
        CancellationToken cancellationToken);

    Task<PagedResult<SalesOrderTotalQuantity>> GetSalesOrderTotalQuantitiesAsync(string salesOrderId,
        CancellationToken cancellationToken);

    Task<SalesOrderJson> GetOrderDetailsAsync(string salesOrderId, CancellationToken cancellationToken);
}