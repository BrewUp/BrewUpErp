using BrewUp.Chat.Facade.Mcp;
using BrewUp.Chat.Facade.Tools;
using BrewUp.Chat.SharedKernel.Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

using ChatResponse = BrewUp.Chat.SharedKernel.Chat.ChatResponse;

namespace BrewUp.Chat.Facade.Chat;

public sealed class BrewUpChatService(
    IChatClient chatClient,
    BrewUpChatTools brewUpChatTools,
    McpServerOptions mcpServerOptions,
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory)
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
                
                   If the user asks about beer availability, stock, or reorder thresholds,
                   call the appropriate warehouse tool.
                   
                   If the user asks about active customers, suppliers, or masterdata info
                   call the appropriate masterdata tool.
                
                   If no tool is suitable, say that the ERP does not expose that information yet.
                
                   Keep the answer concise and business-oriented.
                """),
            new(ChatRole.User, request.Message)
        };

        var options = new ChatOptions
        {
            Tools =
            [
                // Inferred function: beer catalog (no dedicated MCP server yet)
                AIFunctionFactory.Create(brewUpChatTools.GetCatalogBeersAsync),
                AIFunctionFactory.Create(brewUpChatTools.GetOpenSalesOrdersAsync),
                AIFunctionFactory.Create(brewUpChatTools.GetOrdersByCustomerAsync),
                AIFunctionFactory.Create(brewUpChatTools.GetLateSalesOrdersAsync)
            ]
        };

        var response = await chatClient.GetResponseAsync(
            messages,
            options,
            cancellationToken);

        return new ChatResponse(response.Text, request.ConversationId);
    }
}
