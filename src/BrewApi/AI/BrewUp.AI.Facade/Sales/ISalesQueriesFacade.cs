using BrewUp.AI.SharedKernel.Sales;

namespace BrewUp.AI.Facade.Sales;

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
