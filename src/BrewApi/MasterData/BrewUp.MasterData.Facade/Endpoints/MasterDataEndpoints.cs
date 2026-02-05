using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.ReadModel;
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
        
        group.MapPost("/customers", HandlePostCreateCustomer)
            .AddEndpointFilter<ValidationFilter<CreateCustomerJson>>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new customers")
            .WithDescription(
                "Creates a new customer. This endpoint is used to add a new customer.")
            .WithName("CreateCustomer");
        
        group.MapGet("/customers", HandleGetCustomers)
            .Produces<PagedResult<CustomerJson>>()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a list of customers")
            .WithDescription(
                "Get a list of customers.")
            .WithName("GetCustomers");

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
            error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
    
    private static async Task<IResult> HandleGetCustomers(
        IMasterDataFacade masterDataFacade,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var queryResult = await masterDataFacade.GetCustomersAsync(pageNumber, pageSize, cancellationToken);

        return queryResult.Match<IResult>(
            Results.Ok,
            error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
}