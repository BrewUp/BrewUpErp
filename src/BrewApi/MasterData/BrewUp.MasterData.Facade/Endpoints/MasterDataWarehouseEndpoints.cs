using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.MasterData.Facade.Endpoints;

internal static class MasterDataWarehouseEndpoints
{
    internal static WebApplication MapWarehouseEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/v1/warehouse")
            .WithTags("MasterData");
        
        group.MapPost("/", HandlePostWarehouse)
            .AddEndpointFilter<ValidationFilter<CreateWarehouseJson>>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new warehouse")
            .WithDescription(
                "Creates a new warehouse. This endpoint is used to add a new warehouse.")
            .WithName("CreateWarehouse");

        return app;
    }
    
    private static async Task<IResult> HandlePostWarehouse(
        IMasterDataWarehouseFacade warehouseFacade,
        CreateWarehouseJson body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var createResult = await warehouseFacade.CreateWarehouseAsync(body, cancellationToken);

        return createResult.Match<IResult>(
            success =>
            {
                createResult.TryGetValue(out string warehouseId);
                return Results.Created($"/v1/warehouse/{warehouseId}", success);
            }, 
            Results.BadRequest);
    }
}