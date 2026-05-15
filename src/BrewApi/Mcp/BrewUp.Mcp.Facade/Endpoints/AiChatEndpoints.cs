using BrewUp.Mcp.Facade.Chat;
using BrewUp.Mcp.Facade.MasterData;
using BrewUp.Mcp.SharedKernel.Chat;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.Mcp.Facade.Endpoints;

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
        
        return TypedResults.Ok(catalog);
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