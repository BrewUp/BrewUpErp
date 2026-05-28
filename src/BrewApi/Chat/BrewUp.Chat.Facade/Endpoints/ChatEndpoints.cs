using BrewUp.Chat.Facade.Chat;
using BrewUp.Chat.SharedKernel.Chat;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BrewUp.Chat.Facade.Endpoints;

public static class ChatEndpoints
{
    public static WebApplication MapChatEndpoints(this WebApplication app)
    {
        var salesGroup = app.MapGroup("/chat")
            .WithTags("BrewUp-Chat");
        
        salesGroup.MapPost("/", HandleRequestBeersCatalog);
        
        return app;
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