namespace BrewUp.Infrastructure.AzureServiceBus;

public class AzureServiceBusSettings
{
    public required string ConnectionString { get; set; }
    public required string TopicName { get; set; }
    public required string ClientId { get; set; }
    public int MaxConcurrentCalls { get; set; } = 1;
    public bool UseAzureServiceBus { get; set; } = true;
}