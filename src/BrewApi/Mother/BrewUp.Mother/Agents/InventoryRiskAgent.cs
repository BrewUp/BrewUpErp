using BrewUp.Mother.Clients;
using BrewUp.Shared.Messages.Events.Sagas;
using Muflone.Messages.Events;

namespace BrewUp.Mother.Agents;

public sealed class InventoryRiskAgent(
    IMcpToolClient mcpToolClient,
    ILoggerFactory loggerFactory) : IntegrationEventHandlerAsync<SalesOrderConfirmed>(loggerFactory)
{
    public override Task HandleAsync(SalesOrderConfirmed @event, CancellationToken cancellationToken = new ())
    {
        throw new NotImplementedException();
    }
}