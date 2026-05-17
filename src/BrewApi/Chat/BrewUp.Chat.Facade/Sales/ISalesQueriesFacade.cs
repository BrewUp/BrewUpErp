using BrewUp.Chat.SharedKernel.Sales;

namespace BrewUp.Chat.Facade.Sales;

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
