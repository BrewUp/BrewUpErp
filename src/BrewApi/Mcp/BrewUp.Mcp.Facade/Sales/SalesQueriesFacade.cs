using BrewUp.Mcp.SharedKernel.Sales;
using BrewUp.Sales.ReadModel.Services;
using BrewUp.Shared.ExternalContracts.Sales;

namespace BrewUp.Mcp.Facade.Sales;

internal sealed class SalesQueriesFacade(
    ISalesOrderService salesOrderService) : ISalesQueriesFacade
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 250;

    private static readonly HashSet<string> ClosedStatuses =
    [
        "Completed",
        "Closed",
        "Cancelled",
        "Canceled"
    ];

    public async Task<IReadOnlyCollection<SalesOrderSummary>> GetOpenOrdersAsync(
        CancellationToken cancellationToken)
    {
        var orders = await GetAllOrdersAsync(cancellationToken);

        return orders
            .Where(order => !ClosedStatuses.Contains(order.Status))
            .Select(Map)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<SalesOrderSummary>> GetOrdersByCustomerAsync(
        string customerName,
        CancellationToken cancellationToken)
    {
        var orders = await GetAllOrdersAsync(cancellationToken);

        return orders
            .Where(order => order.CustomerName.Contains(
                customerName,
                StringComparison.OrdinalIgnoreCase))
            .Select(Map)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<SalesOrderSummary>> GetLateOrdersAsync(
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        var orders = await GetAllOrdersAsync(cancellationToken);

        return orders
            .Where(order => !ClosedStatuses.Contains(order.Status))
            .Where(order => order.DeliveryDate != DateTime.MaxValue)
            .Where(order => DateOnly.FromDateTime(order.DeliveryDate) < businessDate)
            .Select(Map)
            .ToArray();
    }

    private async Task<IReadOnlyCollection<SalesOrderJson>> GetAllOrdersAsync(
        CancellationToken cancellationToken)
    {
        var result = await salesOrderService.GetSalesOrdersAsync(
            DefaultPageNumber,
            DefaultPageSize,
            cancellationToken);

        if (!result.IsSuccess)
            return [];

        result.TryGetValue(out BrewUp.Shared.ReadModel.PagedResult<SalesOrderJson> page);

        return page.Results.ToArray();
    }

    private static SalesOrderSummary Map(SalesOrderJson order) =>
        new(
            OrderId: order.Id,
            CustomerId: order.CustomerId,
            CustomerName: order.CustomerName,
            Status: order.Status,
            OrderDate: DateOnly.FromDateTime(order.OrderDate),
            RequestedDeliveryDate: order.DeliveryDate == DateTime.MaxValue
                ? null
                : DateOnly.FromDateTime(order.DeliveryDate),
            TotalAmount: CalculateTotalAmount(order.Rows));

    private static decimal CalculateTotalAmount(IEnumerable<SalesOrderRowJson> rows) =>
        rows.Sum(row => row.Quantity.Value * row.Price.Value);
}