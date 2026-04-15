using BrewUp.Shared.ExternalContracts.Warehouse;
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
        
        
        return app;
    }
    
    // private static async Task<IResult> HandlePostWarehouse(
    //     IWarehouseFacade warehouseFacade,
    //     CreateWarehouseJson body,
    //     CancellationToken cancellationToken)
    // {
    //     cancellationToken.ThrowIfCancellationRequested();
    //
    //     var createResult = await warehouseFacade.CreateWarehouseAsync(body, cancellationToken);
    //
    //     return createResult.Match<IResult>(
    //         success =>
    //         {
    //             createResult.TryGetValue(out string orderId);
    //             return Results.Created($"/v1/warehouse/{orderId}", success);
    //         }, 
    //         Results.BadRequest);
    // }
}