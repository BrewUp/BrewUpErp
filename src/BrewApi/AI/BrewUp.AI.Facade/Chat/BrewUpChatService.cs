using BrewUp.AI.Facade.Tools;
using BrewUp.AI.SharedKernel.Chat;
using Microsoft.Extensions.AI;
using ChatResponse = BrewUp.AI.SharedKernel.Chat.ChatResponse;

namespace BrewUp.AI.Facade.Chat;

public sealed class BrewUpChatService(
    IChatClient chatClient,
    BrewUpAiTools tools)
{
    public async Task<ChatResponse> AskAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, """
                You are BrewUp ERP assistant.
                
                   You must answer business questions only by using the available tools.
                   Never answer from memory or assumptions.
                
                   If the user asks about beers, catalog, products, styles or ABV,
                   call GetCatalogBeersAsync.
                
                   If the user asks about open orders, pending orders, active orders,
                   sales order summary, customer orders, late orders, or order status,
                   call the appropriate sales order tool.
                
                   If no tool is suitable, say that the ERP does not expose that information yet.
                
                   Keep the answer concise and business-oriented.
                """),
            new(ChatRole.User, request.Message)
        };

        var options = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(tools.GetCatalogBeersAsync),
                AIFunctionFactory.Create(tools.GetOpenSalesOrdersAsync),
                AIFunctionFactory.Create(tools.GetOrdersByCustomerAsync),
                AIFunctionFactory.Create(tools.GetLateSalesOrdersAsync)
            ]
        };

        var response = await chatClient.GetResponseAsync(
            messages,
            options,
            cancellationToken);

        return new ChatResponse(response.Text, request.ConversationId);
    }
}
