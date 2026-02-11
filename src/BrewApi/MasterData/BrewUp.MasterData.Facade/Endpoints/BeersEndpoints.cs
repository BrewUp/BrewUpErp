using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using BrewUp.Shared.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.MasterData.Facade.Endpoints;

internal static class BeersEndpoints
{
    internal static void MapBeersEndPoints(WebApplication app)
    {
        var group = app.MapGroup("/v1/masterdata/beers")
            .WithTags("Beers");
        
        group.MapPost("/", HandlePostBeer)
            .AddEndpointFilter<ValidationFilter<CreateBeerJson>>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new beer")
            .WithDescription(
                "Creates a new beer. This endpoint is used to add a new beer.")
            .WithName("CreateBeer");
    }
    
    private static async Task<IResult> HandlePostBeer(
        IBeerFacade beerFacade,
        CreateBeerJson body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var createResult = await beerFacade.CreateBeerAsync(body, cancellationToken);

        return createResult.Match<IResult>(
            success =>
            {
                createResult.TryGetValue(out string beerId);
                return Results.Created($"/v1/masterdata/beers/{beerId}", success);
            }, 
            error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
}