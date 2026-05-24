using Microsoft.AspNetCore.SignalR;

namespace BrewUp.Mother.Hubs;

internal sealed class MotherHubHelper(IHubContext<MotherHub> hubContext) : IMotherHubHelper
{
    public async Task TellChildrenThatMotherReceivedIntegrationEvent(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await hubContext.Clients.All.SendAsync("MotherReceivedIntegrationEvent", message, CancellationToken.None)
            .ConfigureAwait(false);
    }

    public async Task TellChildrenThatSalesOrderWasNotFound(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await hubContext.Clients.All.SendAsync("SalesOrderNotFound", message, CancellationToken.None)
            .ConfigureAwait(false);
    }

    public async Task TellChildrenThatSalesOrderWasFound(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await hubContext.Clients.All.SendAsync("SalesOrderFound", message, CancellationToken.None)
            .ConfigureAwait(false);
    }

    public async Task StockRiskDetectionRecommendation(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await hubContext.Clients.All.SendAsync("StockRiskDetectionRecommendation", message, CancellationToken.None)
            .ConfigureAwait(false);
    }
}