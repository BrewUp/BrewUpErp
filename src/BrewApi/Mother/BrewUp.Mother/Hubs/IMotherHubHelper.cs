namespace BrewUp.Mother.Hubs;

public interface IMotherHubHelper
{
    Task TellChildrenThatMotherReceivedIntegrationEvent(string message, CancellationToken cancellationToken);
    Task TellChildrenThatSalesOrderWasNotFound(string message, CancellationToken cancellationToken);
    Task StockRiskDetectionRecommendation(string message, CancellationToken cancellationToken);
}