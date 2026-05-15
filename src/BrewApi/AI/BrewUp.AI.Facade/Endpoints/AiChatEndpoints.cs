using BrewUp.AI.Facade.Chat;
using BrewUp.AI.Facade.MasterData;
using BrewUp.AI.SharedKernel.Chat;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.AI.Facade.Endpoints;

public static class AiChatEndpoints
{
    public static WebApplication MapAiChatEndpoints(this WebApplication app)
    {
        var beersGroup = app.MapGroup("/ai/beers")
            .WithTags("BrewUp-AI");
        var salesGroup = app.MapGroup("/ai/sales")
            .WithTags("BrewUp-AI");
        
        beersGroup.MapGet("/", HandleGetBeersCatalog);
        
        salesGroup.MapPost("/", HandleRequestBeersCatalog);
        
        return app;
    }
    
    private static async Task<IResult> HandleGetBeersCatalog(
        IBeerCatalogQueriesFacade beerCatalogQueriesFacade,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var catalog = await beerCatalogQueriesFacade.GetCatalogBeersAsync(true, cancellationToken);
    
        return catalog.Match<IResult>(
            Results.Ok,
            error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
    
    private static async Task<IResult> HandleRequestBeersCatalog(
        ChatRequest request,
        BrewUpChatService chatService,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = await chatService.AskAsync(
            request,
            cancellationToken);

        return Results.Ok(response);
    }
}