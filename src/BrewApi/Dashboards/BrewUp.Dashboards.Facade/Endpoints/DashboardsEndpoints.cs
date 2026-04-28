using BrewUp.Shared.ExternalContracts.Dashboards;
using BrewUp.Shared.ReadModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.Dashboards.Facade.Endpoints;

public static class DashboardsEndpoints
{
    public static WebApplication MapDashboardsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/dashboards")
            .WithTags("Dashboards");
        
        group.MapGet("/", HandleGetSalesByCustomers)
            .Produces<PagedResult<SalesForCustomerJson>>()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a list of sales by Customer")
            .WithDescription(
                "Get a list of sales by Customer.")
            .WithName("GetSalesByCustomer");
        
        return app;
    }

    private static async Task<IResult> HandleGetSalesByCustomers(
        IDashboardsFacade dashboardsFacade,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var queryResult =
            await dashboardsFacade.GetSalesByCustomerAsync(pageNumber, pageSize, cancellationToken);
    
        return queryResult.Match<IResult>(
            Results.Ok,
            error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
}