namespace Muflone.Transport.RabbitMQ.Models;

public class ConsumerConfiguration
{
	public string QueueName { get; set; } = string.Empty;
	public string ResourceKey { get; set; } = string.Empty;
}