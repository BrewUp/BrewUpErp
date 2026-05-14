using Muflone.Transport.RabbitMQ.Abstracts;
using RabbitMQ.Client;

namespace Muflone.Transport.RabbitMQ.Factories;

public sealed class ServiceBusSenderFactory : IServiceBusSenderFactory
{
	private readonly IConnection _connection;
	//protected bool IsConnected => Connection is { IsOpen: true };

	public ServiceBusSenderFactory(IConnection connection)
	{
		_connection = connection ?? throw new NullReferenceException(nameof(Exception));
	}
}