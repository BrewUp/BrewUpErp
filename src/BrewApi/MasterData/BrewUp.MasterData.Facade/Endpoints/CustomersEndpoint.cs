using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.ReadModel;
using BrewUp.Shared.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.MasterData.Facade.Endpoints;

internal static class CustomersEndpoint
{
    internal static void MapCustomersEndPoints(WebApplication app)
    {
        var group = app.MapGroup("/v1/masterdata/customers")
            .WithTags("Customers");
        
        group.MapPost("/", HandlePostCustomer)
            .AddEndpointFilter<ValidationFilter<CreateCustomerJson>>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new customers")
            .WithDescription(
                "Creates a new customer. This endpoint is used to add a new customer.")
            .WithName("CreateCustomer");
        
        group.MapPut("/{customerId}", HandlePutCustomer)
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Save customer")
            .WithDescription(
                "Save full details of a customer.")
            .WithName("SaveCustomer");
        
        group.MapDelete("/{customerId}", HandleDeleteCustomer)
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Delete customer")
            .WithDescription(
                "Delete a customer.")
            .WithName("DeleteCustomer");
        
        group.MapGet("/", HandleGetCustomers)
            .Produces<PagedResult<CustomerJson>>()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a list of customers")
            .WithDescription(
                "Get a list of customers.")
            .WithName("GetCustomers");
        
        group.MapGet("/{customerId}", HandleGetCustomerById)
            .Produces<CustomerJson>()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a customer details")
            .WithDescription(
                "Get full details of a customer.")
            .WithName("GetCustomerById");
    }
    
    private static async Task<IResult> HandlePostCustomer(
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
                return Results.Created($"/v1/masterdata/customers/{customerId}", success);
            }, 
            error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
    
    private static async Task<IResult> HandlePutCustomer(
        IMasterDataFacade masterDataFacade,
        string customerId,
        EditCustomerJson body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var createResult = await masterDataFacade.SaveCustomerAsync(body, cancellationToken);

        return createResult.Match<IResult>(
            success => Results.Accepted($"/v1/masterdata/customers/{body.CustomerId}", success), 
            error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
    
    private static async Task<IResult> HandleDeleteCustomer(
        IMasterDataFacade masterDataFacade,
        string customerId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var createResult = await masterDataFacade.DeleteCustomerAsync(customerId, cancellationToken);

        return createResult.Match<IResult>(
            success => Results.Accepted(), 
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
    
    private static async Task<IResult> HandleGetCustomerById(
        IMasterDataFacade masterDataFacade,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var queryResult = await masterDataFacade.GetCustomerByIdAsync(customerId, cancellationToken);

        return queryResult.Match<IResult>(
            Results.Ok,
            error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
}