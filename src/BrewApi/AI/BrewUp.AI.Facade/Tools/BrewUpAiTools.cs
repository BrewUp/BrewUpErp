using System.ComponentModel;
using BrewUp.AI.Facade.MasterData;
using BrewUp.AI.Facade.Sales;
using BrewUp.AI.SharedKernel.Catalog;
using BrewUp.AI.SharedKernel.Sales;
using Lena.Core;

namespace BrewUp.AI.Facade.Tools;

public sealed class BrewUpAiTools(
    IBeerCatalogQueriesFacade beerCatalogQueries,
    ISalesQueriesFacade salesOrderQueries)
{
    [Description("Use this tool whenever the user asks for the active beers available or beers catalog.")]
    public Task<Result<IReadOnlyCollection<BeerCatalogItem>>> GetCatalogBeersAsync(
        CancellationToken cancellationToken = default)
        => beerCatalogQueries.GetCatalogBeersAsync(activeOnly: true, cancellationToken);

    [Description("Use this tool whenever the user asks for open sales orders, pending orders, active orders, orders not completed, or a sales order summary.")]
    public Task<IReadOnlyCollection<SalesOrderSummary>> GetOpenSalesOrdersAsync(
        CancellationToken cancellationToken = default)
        => salesOrderQueries.GetOpenOrdersAsync(cancellationToken);

    [Description("Use this tool whenever the user asks for sales orders for a customer, searching by customer name.")]
    public Task<IReadOnlyCollection<SalesOrderSummary>> GetOrdersByCustomerAsync(
        [Description("The customer name, or part of the customer name.")]
        string customerName,
        CancellationToken cancellationToken = default)
        => salesOrderQueries.GetOrdersByCustomerAsync(customerName, cancellationToken);

    [Description("Use this tool whenever the user asks for sales orders whose requested delivery date is before the supplied business date and are still not completed.")]
    public Task<IReadOnlyCollection<SalesOrderSummary>> GetLateSalesOrdersAsync(
        [Description("Business date in yyyy-MM-dd format.")]
        DateOnly businessDate,
        CancellationToken cancellationToken = default)
        => salesOrderQueries.GetLateOrdersAsync(businessDate, cancellationToken);
}
