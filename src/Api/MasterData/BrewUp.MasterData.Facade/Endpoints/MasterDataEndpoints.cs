using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.MasterData.Facade.Endpoints;

public static class MasterDataEndpoints
{
    public static WebApplication MapMasterDataEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/masterdata")
            .WithTags("MasterData");
        
        group.MapPost("/", HandlePostCreateCustomer)
            .AddEndpointFilter<ValidationFilter<CreateCustomerJson>>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new customers")
            .WithDescription(
                "Creates a new customer. This endpoint is used to add a new customer.")
            .WithName("CreateCustomer");

        return app;
    }
    
    private static async Task<IResult> HandlePostCreateCustomer(
        IMasterDataFacade masterDataFacade,
        CreateCustomerJson body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var createResult = await masterDataFacade.CreateCustomerAsync(body, cancellationToken);

        return createResult.Match<IResult>(
            success =>
            {
                createResult.TryGetValue(out string customerId);
                return Results.Created($"/v1/masterdata/{customerId}", success);
            }, 
            failure => Results.Problem("An error occurred while creating the customer.", statusCode: StatusCodes.Status500InternalServerError));
    }
}