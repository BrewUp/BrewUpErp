using BrewUp.Mother.Clients;
using BrewUp.Mother.CustomTypes;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.Messages.Events.Sales;
using Muflone.Messages.Events;

namespace BrewUp.Mother.Agents;

public sealed class InventoryRiskAgent(
    IMcpToolClient mcpToolClient,
    IRecommendationWriter recommendationWriter,
    ILoggerFactory loggerFactory) : IntegrationEventHandlerAsync<SalesOrderCreatedIntegrationEvent>(loggerFactory)
{
    private readonly ILogger<InventoryRiskAgent> _logger = loggerFactory.CreateLogger<InventoryRiskAgent>();
    
    public override async Task HandleAsync(SalesOrderCreatedIntegrationEvent @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        _logger.LogInformation(
            "BrewUp.Mother InventoryRiskAgent received SalesOrderConfirmed {SalesOrderId}",
            @event.AggregateId.Value);

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

            return;
        }

        foreach (var row in order.Rows)
        {
            var availability = await mcpToolClient.CallToolAsync<AvailabilityWithThresholdJson>(
                serverName: "warehouse",
                toolName: "get_beer_availability",
                arguments: new
                {
                    beerId = row.BeerId
                },
                cancellationToken);

            if (availability is null)
                continue;

            var residualAvailability =
                availability.Quantity - row.Quantity.Value;

            if (residualAvailability >= availability.ReorderThreshold)
                continue;

            var recommendation = new StockRiskDetected(
                SalesOrderId: order.Id,
                BeerId: row.BeerId,
                BeerName: row.BeerName,
                RequiredQuantity: row.Quantity.Value,
                AvailableQuantity: availability.Quantity,
                ReorderThreshold: availability.ReorderThreshold,
                Reason:
                "The confirmed sales order would reduce warehouse availability below the reorder threshold.");

            await recommendationWriter.WriteAsync(
                recommendation,
                cancellationToken);
        }
    }
}