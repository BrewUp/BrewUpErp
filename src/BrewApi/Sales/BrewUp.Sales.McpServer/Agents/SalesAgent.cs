using BrewUp.Mother.McpClients;
using BrewUp.Shared.Agents;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.Messages.Events.Mother;
using BrewUp.Shared.Messages.Events.Sales;

namespace BrewUp.Sales.McpServer.Agents;

public sealed class SalesAgent(IMcpToolClient mcpToolClient,
    ILoggerFactory loggerFactory) : AgentBase<SalesOrderCreatedIntegrationEvent, SalesOrderAssessment>
{
    private readonly ILogger<SalesAgent> _logger = loggerFactory.CreateLogger<SalesAgent>();

    public override async Task<SalesOrderAssessment> HandleAsync(SalesOrderCreatedIntegrationEvent @event,
        CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        _logger.LogInformation(
            "BrewUp.Sales.McpServer.Agents SalesAgent received SalesOrderCreatedIntegrationEvent {SalesOrderId}",
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

            return new SalesOrderAssessment(
                new IntegrationId(Guid.CreateVersion7().ToString()),
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                [],
                priority: "Normal",
                reason: "No SalesOrder was found!");
        }

        var totalAmount = order.Rows.Sum(r => r.Quantity.Value * r.Price.Value);
        return new SalesOrderAssessment(
            new IntegrationId(Guid.CreateVersion7().ToString()),
            order.Id,
            order.CustomerId,
            order.CustomerName,
            totalAmount,
            order.Rows.ToList().AsReadOnly(),
            priority: totalAmount > 5000 ? "High" : "Normal",
            reason: "Priority calculated from order amount.");
    }


}