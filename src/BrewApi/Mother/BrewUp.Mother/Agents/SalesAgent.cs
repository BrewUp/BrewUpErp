using BrewUp.Mother.Hubs;
using BrewUp.Mother.McpClients;
using BrewUp.Shared.ExternalContracts.Mother;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.Messages.Events.Sales;
using Muflone.Messages.Events;

namespace BrewUp.Mother.Agents;

public sealed class SalesAgent(IMcpToolClient mcpToolClient,
    IMotherHubHelper hubHelper,
    ILoggerFactory loggerFactory) : IntegrationEventHandlerAsync<SalesOrderCreatedIntegrationEvent>(loggerFactory)
{
    private readonly ILogger<SalesAgent> _logger = loggerFactory.CreateLogger<SalesAgent>();
    
    public override async Task HandleAsync(SalesOrderCreatedIntegrationEvent @event,
        CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        _logger.LogInformation(
            "BrewUp.Mother InventoryRiskAgent received SalesOrderConfirmed {SalesOrderId}",
            @event.AggregateId.Value);
        await hubHelper.TellChildrenThatMotherReceivedIntegrationEvent(
            $"BrewUp.Mother InventoryRiskAgent received SalesOrderConfirmed {@event.AggregateId.Value}",
            cancellationToken);

        var order = await mcpToolClient.CallToolAsync<SalesOrderJson>(
            serverName: "sales",
            toolName: "get_sales_order_details",
            arguments: new
            {
                salesOrderId = @event.AggregateId.Value
            },
            cancellationToken);

        if (order is null)
        {
            _logger.LogWarning(
                "Sales order {SalesOrderId} not found by Sales MCP",
                @event.AggregateId.Value);

            await hubHelper.TellChildrenThatSalesOrderWasNotFound(
                $"Sales order {@event.AggregateId.Value} not found by Sales MCP", cancellationToken);

            return;
        }

        var totalAmount = order.Rows.Sum(r => r.Quantity.Value * r.Price.Value);
        var items = order.Rows.ToList().AsReadOnly();
        SalesOrderAssessment salesOrderAssessment = new (
            order.Id,
            order.CustomerId,
            order.CustomerName,
            totalAmount,
            order.Rows.ToList().AsReadOnly(),
            Priority: totalAmount > 5000 ? "High" : "Normal",
            Reason: "Priority calculated from order amount.");
    }
}