using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrewUp.Sales.ReadModel.Services;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Sales.McpServer;

internal sealed class McpSalesFacade(
    ISalesOrderService salesOrderService) : IMcpSalesFacade
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

    public async Task<CustomerTotalPurchased> GetCustomerTotalPurchasedAsync(string customerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var result = await salesOrderService.GetCustomerTotalPurchasedAsync(new CustomerId(customerId), cancellationToken);

        if (!result.IsSuccess)
            return new CustomerTotalPurchased(customerId, string.Empty, 0);

        return result.TryGetValue(out CustomerTotalPurchased totalPurchased) 
            ? totalPurchased 
            : new CustomerTotalPurchased(customerId, string.Empty, 0);
    }

    public async Task<PagedResult<SalesOrderTotalQuantity>> GetSalesOrderTotalQuantitiesAsync(string salesOrderId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var queryResult = await salesOrderService.GetSalesOrderTotalQuantitiesAsync(salesOrderId, cancellationToken);
        if (queryResult.IsError)
            return new PagedResult<SalesOrderTotalQuantity>([], 0, 0, 0);
        
        queryResult.TryGetValue(out PagedResult<SalesOrderTotalQuantity> pagedResult);
        return pagedResult;
    }

    public async Task<SalesOrderJson> GetOrderDetailsAsync(string salesOrderId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var orderResult = await salesOrderService.GetSalesOrderByIdAsync(salesOrderId, cancellationToken);
        if (orderResult.IsError)
            return new SalesOrderJson();
        
        return orderResult.TryGetValue(out SalesOrderJson result) 
            ? result 
            : new SalesOrderJson();
    }

    public async Task<IReadOnlyCollection<SalesOrderSummary>> GetOrdersByBeerAsync(string beerName, CancellationToken cancellationToken)
    {
        var orders = await GetAllOrdersAsync(cancellationToken);

        return orders
            .Where(order => order.Rows.Any(row => row.BeerName.Contains(
                beerName,
                StringComparison.OrdinalIgnoreCase)))
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

        result.TryGetValue(out PagedResult<SalesOrderJson> page);

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