using BrewUp.Chat.Facade.Chat;
using BrewUp.Chat.Facade.MasterData;
using BrewUp.Chat.SharedKernel.Chat;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.Chat.Facade.Endpoints;

public static class ChatEndpoints
{
    public static WebApplication MapChatEndpoints(this WebApplication app)
    {
        var beersGroup = app.MapGroup("/chat/beers")
            .WithTags("BrewUp-Chat");
        var salesGroup = app.MapGroup("/chat/sales")
            .WithTags("BrewUp-Chat");
        
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