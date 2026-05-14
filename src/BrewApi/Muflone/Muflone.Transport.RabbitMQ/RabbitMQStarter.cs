using Microsoft.Extensions.Hosting;

namespace Muflone.Transport.RabbitMQ;

public class RabbitMQStarter(IEnumerable<IConsumer> consumers) : IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken)
	{
		foreach (var consumer in consumers) consumer.StartAsync(cancellationToken);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}