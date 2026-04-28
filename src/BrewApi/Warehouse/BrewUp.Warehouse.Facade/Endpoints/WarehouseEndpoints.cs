using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.ReadModel;
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
}