using BrewUp.Mcp.SharedKernel.Sales;

namespace BrewUp.Mcp.Facade.Sales;

public interface ISalesQueriesFacade
{
    Task<IReadOnlyCollection<SalesOrderSummary>> GetOpenOrdersAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SalesOrderSummary>> GetOrdersByCustomerAsync(
        string customerName,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SalesOrderSummary>> GetLateOrdersAsync(
        DateOnly businessDate,
        CancellationToken cancellationToken);
}
