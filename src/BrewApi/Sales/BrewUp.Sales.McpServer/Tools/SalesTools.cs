using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Server;

namespace BrewUp.Sales.McpServer.Tools;

[McpServerToolType]
public sealed class SalesTools(
    IMcpSalesFacade mcpSalesFacade)
{
    [McpServerTool(Name = "get_open_sales_orders")]
    [Description("Returns the currently open sales orders.")]
    public async Task<object> GetOpenSalesOrders(CancellationToken cancellationToken)
        => await mcpSalesFacade.GetOpenOrdersAsync(cancellationToken);
    
    [McpServerTool(Name = "get_sales_order_details")]
    [Description("Returns the details of a sales order, searching by sales order id. Use this tool when someone asks for details of a sales order.")]
    public async Task<object> GetSalesOrderDetails(
        [Description("The sales order id, or part of the sales order id.")]
        string salesOrderId,
        CancellationToken cancellationToken)
        => await mcpSalesFacade.GetOrderDetailsAsync(salesOrderId, cancellationToken);

    [McpServerTool(Name = "get_orders_by_customer")]
    [Description("Returns the sales orders for a customer, searching by customer name.")]
    public async Task<object> GetOrdersByCustomer(
        [Description("The customer name, or part of the customer name.")]
        string customerName,
        CancellationToken cancellationToken)
        => await mcpSalesFacade.GetOrdersByCustomerAsync(customerName, cancellationToken);
    
    [McpServerTool(Name = "get_orders_by_beer")]
    [Description("Returns the sales orders for a beer, searching by beer name.")]
    public async Task<object> GetOrdersByBeer(
        [Description("The beer name, or part of the beer name.")]
        string beerName,
        CancellationToken cancellationToken)
        => await mcpSalesFacade.GetOrdersByBeerAsync(beerName, cancellationToken);

    [McpServerTool(Name = "get_late_sales_orders")]
    [Description("Returns late sales orders at the supplied business date.")]
    public async Task<object> GetLateSalesOrders(
        [Description("Business date in yyyy-MM-dd format.")]
        DateOnly businessDate,
        CancellationToken cancellationToken)
        => await mcpSalesFacade.GetLateOrdersAsync(businessDate, cancellationToken);
    
    [McpServerTool(Name = "get_customer_total_purchased")]
    [Description("Returns the total purchases per customer")]
    public async Task<object> GetCustomerTotalPurchased(
        [Description("The customerId, or part of the customer id.")]
        string customerId,
        CancellationToken cancellationToken)
        => await mcpSalesFacade.GetCustomerTotalPurchasedAsync(customerId, cancellationToken);
    
    [McpServerTool(Name = "get_sales_order_total_quantities")]
    [Description("Returns the total beers for order")]
    public async Task<object> GetSalesOrderTotalQuantities(
        [Description("The salesOrderId.")]
        string salesOrderId,
        CancellationToken cancellationToken)
        => await mcpSalesFacade.GetSalesOrderTotalQuantitiesAsync(salesOrderId, cancellationToken);
}
