using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.ReadModel;
using BrewUp.Shared.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.Warehouse.Facade.Endpoints;

public static class WarehouseEndpoints
{
    public static WebApplication MapWarehouseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/warehouse")
            .WithTags("Warehouse");
        
        group.MapGet("/", HandleGetShipmentOrders)
            .Produces<PagedResult<ShipmentJson>>()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a list of shipment orders")
            .WithDescription(
                "Get a list of shipment orders.")
            .WithName("GetShipmentOrders");

        group.MapPost("/", HandleAddItemStocks)
            .AddEndpointFilter<ValidationFilter<WarehouseJson>>()
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Add item stocks to a warehouse")
            .WithDescription(
                "Adds item stocks to an existing warehouse. This endpoint is used to update the stock of items in a warehouse.")
            .WithName("AddItemStocks");

        return app;
    }
    
    private static async Task<IResult> HandleGetShipmentOrders(
        IWarehouseFacade warehouseFacade,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var getResult = await warehouseFacade.GetShipmentOrdersAsync(pageNumber, pageSize, cancellationToken);

        return getResult.Match<IResult>(
            success => Results.Ok(success),
            Results.BadRequest);
    }

    private static async Task<IResult> HandleAddItemStocks(
        IWarehouseFacade warehouseFacade,
        WarehouseJson body,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await warehouseFacade.AddItemStocksAsync(body, cancellationToken);

        return result.Match<IResult>(
            success =>
            {
                return Results.Created($"/v1/warehouse", success);
            },
            Results.BadRequest);
    }
}