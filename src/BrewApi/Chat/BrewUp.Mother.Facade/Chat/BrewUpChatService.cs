using BrewUp.Mother.Facade.Agents;
using BrewUp.Mother.Facade.Mcp;
using BrewUp.Mother.SharedKernel.Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ChatResponse = BrewUp.Mother.SharedKernel.Chat.ChatResponse;

namespace BrewUp.Mother.Facade.Chat;

public sealed class BrewUpChatService(
    IChatClient chatClient,
    IMcpToolsProvider mcpToolsProvider,
    MotherCoordinator motherCoordinator,
    ILogger<BrewUpChatService> logger)
{
     private const string SystemPrompt = """
         You are BrewUp ERP assistant.

            You must answer business questions only by using the available tools.
            Never answer from memory or assumptions.

            If no specific tool is suitable, try to coordinate more tools to response.
            If you don't have success say that the BrewUp ERP does not expose that information yet.

            Keep the answer concise and business-oriented.

            If the user asks about customers, suppliers, beers, catalog, products, styles or ABV,
            call the appropriate masterData tool.

            If the user asks about open orders, pending orders, active orders,
            sales order summary, customer orders, late orders, or order status,
            call the appropriate sales order tool.

            If the user asks about beer availability, stock, or reorder thresholds,
            call the appropriate warehouse tool.
            
            If the user asks about:
            - company policies
            - business procedures
            - operational guidelines
            - product documentation
            - business rules
            - brewery processes
            - quality standards
            - organizational information
            - how BrewUp works
            - general company knowledge
            
            use the knowledge tool.
            
            Use operational tools when the user asks about current business data.
         
            Examples:
            - open orders
            - customer orders
            - stock availability
            - reorder thresholds
            - product catalog
            
            Use the knowledge tool when the user asks about documentation, procedures, policies, business rules, or general company knowledge.
            
            Examples:
            - What is BrewUp?
            - How does inventory management work?
            - What are the quality standards?
            - How does the sales order lifecycle work?
            - How is beer produced?
            
            Knowledge information and operational data are different concepts.
         
            Use knowledge tools for documented business knowledge.
            Use ERP tools for current operational information.
            Combine both when required.
            
            For what-if analysis, simulations, impact assessment, cross-context reasoning,
            recommendations, or questions starting with "what happens if" or "what if",
            use all the tools to generate the answer.
            
            For cross-boundaries scenarios analysis and cross-bounded-context reasoning.
            Use all required tools when the user asks hypothetical questions such as:
            - What if I receive an order of 100 bottles of Muflone Weiss?
            - What happens to the warehouse if a customer orders 100 bottles of IPA?
            - Would this order create a stock risk?
            - Would this scenario require a reorder?
            
            Use direct bounded-context tools for simple lookups.
            Use all required tools for simulations, what-if analysis, cross-context reasoning, and recommendations.
            
            Do not invent business data.
         """;

    public async Task<ChatResponse> AskAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (motherCoordinator.CanCoordinate(request))
            return await motherCoordinator.CoordinateAsync(request, cancellationToken);

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
            tools = [];
        }

        // function inferred
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
