using BrewUp.Chat.Facade.Mcp;
using BrewUp.Chat.SharedKernel.Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using ChatResponse = BrewUp.Chat.SharedKernel.Chat.ChatResponse;

namespace BrewUp.Chat.Facade.Chat;

public sealed class BrewUpChatService(
    IChatClient chatClient,
    IMcpToolsProvider mcpToolsProvider,
    ILogger<BrewUpChatService> logger)
{
    private const string SystemPrompt = """
        You are BrewUp ERP assistant.

           You must answer business questions only by using the available tools.
           Never answer from memory or assumptions.

           If no tool is suitable, say that the ERP does not expose that information yet.

           Keep the answer concise and business-oriented.

           If the user asks about customers, suppliers, beers, catalog, products, styles or ABV,
           call the appropriate masterData tool.

           If the user asks about open orders, pending orders, active orders,
           sales order summary, customer orders, late orders, or order status,
           call the appropriate sales order tool.

           If the user asks about beer availability, stock, or reorder thresholds,
           call the appropriate warehouse tool.
        """;

    public async Task<ChatResponse> AskAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, request.Message)
        };

        IReadOnlyList<AITool> tools;
        try
        {
            tools = await mcpToolsProvider.GetToolsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load MCP tools.");
            tools = Array.Empty<AITool>();
        }

        var options = new ChatOptions
        {
            Tools = tools.Count > 0 ? tools.ToList() : null
        };

        try
        {
            var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);
            return new ChatResponse(response.Text, request.ConversationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat completion failed for conversation {ConversationId}.", request.ConversationId);
            throw;
        }
    }
}
