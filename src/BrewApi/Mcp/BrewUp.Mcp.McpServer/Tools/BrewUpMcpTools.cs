using System.ComponentModel;
using BrewUp.Mcp.Facade.MasterData;
using BrewUp.Mcp.Facade.Sales;
using ModelContextProtocol.Server;

namespace BrewUp.Mcp.McpServer.Tools;

[McpServerToolType]
public sealed class BrewUpMcpTools(
    IBeerCatalogQueriesFacade beerCatalogQueries,
    ISalesQueriesFacade salesOrderQueries)
{
    [McpServerTool(Name = "get_catalog_beers")]
    [Description("Returns the active beers available in the BrewUp beer catalog.")]
    public async Task<object> GetCatalogBeers(CancellationToken cancellationToken)
        => await beerCatalogQueries.GetCatalogBeersAsync(true, cancellationToken);

    [McpServerTool(Name = "get_open_sales_orders")]
    [Description("Returns the currently open sales orders.")]
    public async Task<object> GetOpenSalesOrders(CancellationToken cancellationToken)
        => await salesOrderQueries.GetOpenOrdersAsync(cancellationToken);

    [McpServerTool(Name = "get_orders_by_customer")]
    [Description("Returns the sales orders for a customer, searching by customer name.")]
    public async Task<object> GetOrdersByCustomer(
        [Description("The customer name, or part of the customer name.")]
        string customerName,
        CancellationToken cancellationToken)
        => await salesOrderQueries.GetOrdersByCustomerAsync(customerName, cancellationToken);

    [McpServerTool(Name = "get_late_sales_orders")]
    [Description("Returns late sales orders at the supplied business date.")]
    public async Task<object> GetLateSalesOrders(
        [Description("Business date in yyyy-MM-dd format.")]
        DateOnly businessDate,
        CancellationToken cancellationToken)
        => await salesOrderQueries.GetLateOrdersAsync(businessDate, cancellationToken);
}
