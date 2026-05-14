using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Messages;
using Muflone.Persistence;
using Muflone.Transport.RabbitMQ.Abstracts;
using Muflone.Transport.RabbitMQ.Consumers;
using Muflone.Transport.RabbitMQ.Factories;
using Muflone.Transport.RabbitMQ.Models;

namespace Muflone.Transport.RabbitMQ;

public static class TransportRabbitMQHelper
{
	public static IServiceCollection AddMufloneTransportRabbitMQ(this IServiceCollection services,
		ILoggerFactory loggerFactory,
		RabbitMQConfiguration rabbitMQConfiguration)
	{
		services.AddSingleton(new RabbitMQConnectionFactory(rabbitMQConfiguration, loggerFactory));
		services.AddSingleton(rabbitMQConfiguration);
		services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
		services.AddSingleton<IServiceBus, ServiceBus>();
		services.AddSingleton<IEventBus, ServiceBus>();
		services.AddSingleton<IMessageSubscriber, RabbitMQSubscriber>();
		services.AddHostedService<RabbitMQStarter>();
		services.AddHostedService<MessageHandlersStarter>();

		return services;
	}

	public static IServiceCollection AddMufloneRabbitMQConsumers(this IServiceCollection services,
		IEnumerable<IConsumer> consumers)
	{
		services.AddSingleton(consumers);
		return services;
	}
}