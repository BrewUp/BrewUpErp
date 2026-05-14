using Microsoft.Extensions.Logging;
using Muflone.Transport.RabbitMQ.Abstracts;
using Muflone.Transport.RabbitMQ.Models;
using Polly;
using RabbitMQ.Client;

namespace Muflone.Transport.RabbitMQ.Factories;

public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory
{
	public IConnection Connection { get; private set; } = default!;

	protected bool IsConnected => Connection is { IsOpen: true };
	public string ExchangeCommandsName { get; }
	public string ExchangeEventsName { get; }
	public string ClientId { get; }

	private readonly RabbitMQConfiguration _rabbitMqConfiguration;
	private readonly ILogger _logger;

	public RabbitMQConnectionFactory(RabbitMQConfiguration rabbitMqConfiguration, ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger(GetType()) ?? throw new ArgumentNullException(nameof(loggerFactory));
		_rabbitMqConfiguration =
				rabbitMqConfiguration ?? throw new ArgumentNullException(nameof(rabbitMqConfiguration));
		ExchangeCommandsName = rabbitMqConfiguration.ExchangeCommandsName;
		ExchangeEventsName = rabbitMqConfiguration.ExchangeEventsName;
		ClientId = rabbitMqConfiguration.ClientId;

		TryCreateConnectionAsync().GetAwaiter().GetResult();
	}

	public Task StartAsync()
	{
		return TryCreateConnectionAsync();
	}

	public Task<IChannel> CreateChannelAsync()
	{
		if (!IsConnected)
			throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");

		return Connection.CreateChannelAsync();
	}


	private async Task TryCreateConnectionAsync()
	{
		if (IsConnected)
			return;

		var policy = Policy
				.Handle<Exception>()
				.WaitAndRetryAsync(5,
						retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
						(ex, timeSpan, context) =>
						{
							_logger.LogError(ex, $"An exception has occurred while opening RabbitMQ connection: {ex.Message}");
						});

		Connection = await policy.ExecuteAsync(() => CreateConnectionAsync(_rabbitMqConfiguration));
		Connection.ConnectionShutdownAsync += (s, e) => TryCreateConnectionAsync();
		Connection.CallbackExceptionAsync += (s, e) => TryCreateConnectionAsync();
		Connection.ConnectionBlockedAsync += (s, e) => TryCreateConnectionAsync();
	}

	private static Task<IConnection> CreateConnectionAsync(RabbitMQConfiguration rabbitMqConfiguration)
	{
		var connectionFactory = new ConnectionFactory
		{
			HostName = rabbitMqConfiguration.HostName,
			UserName = rabbitMqConfiguration.UserName,
			Password = rabbitMqConfiguration.Password,
			VirtualHost = rabbitMqConfiguration.VirtualHost,
			Port = AmqpTcpEndpoint.UseDefaultPort
		};

		return connectionFactory.CreateConnectionAsync();
	}
}