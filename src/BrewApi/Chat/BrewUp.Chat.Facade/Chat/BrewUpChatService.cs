using BrewUp.Chat.Facade.Mcp;
using BrewUp.Chat.SharedKernel.Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

using ChatResponse = BrewUp.Chat.SharedKernel.Chat.ChatResponse;

namespace BrewUp.Chat.Facade.Chat;

public sealed class BrewUpChatService(
    IChatClient chatClient,
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
                
                   If no tool is suitable, say that the ERP does not expose that information yet.
                
                   Keep the answer concise and business-oriented.
                   
                   If the user asks about customers, suppliers, beers, catalog, products, styles or ABV,
                   call the appropriate masterData tool.

                   If the user asks about open orders, pending orders, active orders,
                   sales order summary, customer orders, late orders, or order status,
                   call the appropriate sales order tool.

                   If the user asks about beer availability, stock, or reorder thresholds,
                   call the appropriate warehouse tool.
                """),
            new(ChatRole.User, request.Message)
        };

        // Clients must stay alive for the entire request so tool invocations succeed.
        await using var masterDataClient = await CreateMcpClientAsync("MasterData", mcpServerOptions.MasterDataUrl, cancellationToken);
        await using var salesClient = await CreateMcpClientAsync("Sales", mcpServerOptions.SalesUrl, cancellationToken);
        await using var warehouseClient = await CreateMcpClientAsync("Warehouse", mcpServerOptions.WarehouseUrl, cancellationToken);

        var masterDataTools = await masterDataClient.ListToolsAsync(cancellationToken: cancellationToken);
        var salesTools= await salesClient.ListToolsAsync(cancellationToken: cancellationToken);
        var warehouseTools= await warehouseClient.ListToolsAsync(cancellationToken: cancellationToken);

        var options = new ChatOptions
        {
            Tools =
            [
                ..masterDataTools,
                ..salesTools,
                ..warehouseTools
            ]
        };

        var response = await chatClient.GetResponseAsync(
            messages,
            options,
            cancellationToken);

        return new ChatResponse(response.Text, request.ConversationId);
    }

    private async Task<McpClient> CreateMcpClientAsync(string mcpName, string mcpUrl, CancellationToken cancellationToken)
        => await McpClient.CreateAsync(
            new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(mcpUrl),
                Name = mcpName
            }, httpClientFactory.CreateClient(), loggerFactory),
            loggerFactory: loggerFactory,
            cancellationToken: cancellationToken);
}
