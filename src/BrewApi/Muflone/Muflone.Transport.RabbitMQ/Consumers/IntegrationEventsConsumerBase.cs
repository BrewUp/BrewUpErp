using Microsoft.Extensions.Logging;
using Muflone.Messages;
using Muflone.Messages.Events;
using Muflone.Persistence;
using Muflone.Transport.RabbitMQ.Abstracts;
using Muflone.Transport.RabbitMQ.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Muflone.Transport.RabbitMQ.Consumers;

public abstract class IntegrationEventsConsumerBase<T> : ConsumerBase, IIntegrationEventConsumer<T>, IAsyncDisposable
	where T : IntegrationEvent
{
	private readonly ISerializer _messageSerializer;
	private readonly ConsumerConfiguration _configuration;
	private readonly IRabbitMQConnectionFactory _connectionFactory;
	private IChannel _channel;
	protected abstract IEnumerable<IIntegrationEventHandlerAsync<T>> HandlersAsync { get; }

	protected IntegrationEventsConsumerBase(IRabbitMQConnectionFactory mufloneConnectionFactory,
		ILoggerFactory loggerFactory)
		: this(new ConsumerConfiguration(), mufloneConnectionFactory, loggerFactory)
	{
	}

	protected IntegrationEventsConsumerBase(ConsumerConfiguration configuration,
		IRabbitMQConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
		: base(loggerFactory)
	{
		_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
		_messageSerializer = new Serializer();
		_channel = null!;

		GetQueueName(configuration);
		_configuration = configuration;
	}

	public async Task ConsumeAsync(T message, CancellationToken cancellationToken = default)
	{
		foreach (var handlerAsync in HandlersAsync)
			await handlerAsync.HandleAsync(message, cancellationToken);
	}

	public async Task StartAsync(CancellationToken cancellationToken = default)
	{
		await InitChannelAsync();
		await InitSubscriptionAsync();
	}

	public async Task StopAsync(CancellationToken cancellationToken = default)
	{
		await StopChannelAsync();
	}

	private async Task InitChannelAsync()
	{
		Logger.LogInformation(
			$"initializing retry queue '{_configuration.QueueName}' on exchange '{_connectionFactory.ExchangeEventsName}'...");

		_channel = await _connectionFactory.CreateChannelAsync();
		await _channel.ExchangeDeclareAsync(_connectionFactory.ExchangeEventsName, ExchangeType.Topic, true);
		await _channel.QueueDeclareAsync(_configuration.QueueName, true, false, false);
		await _channel.QueueBindAsync(_configuration.QueueName, _connectionFactory.ExchangeEventsName,
			_configuration.ResourceKey);

		_channel.CallbackExceptionAsync += async (_, e) =>
		{
			Logger.LogWarning($"Channel exception: {e.Exception.Message}");
			await OnChannelExceptionAsync(e);
		};
	}

	private async Task StopChannelAsync()
	{
		if (!_channel.IsOpen)
			return;

		await _channel.CloseAsync();
		_channel.Dispose();
	}

	private async Task OnChannelExceptionAsync(CallbackExceptionEventArgs ea)
	{
		Logger.LogError(ea.Exception, $"The RabbitMQ Channel has encountered an error: {ea.Exception.Message}");

		await InitChannelAsync();
		await InitSubscriptionAsync();
	}

	private async Task InitSubscriptionAsync()
	{
		var consumer = new AsyncEventingBasicConsumer(_channel);
		consumer.ReceivedAsync += OnMessageReceivedAsync;
		await _channel.BasicConsumeAsync(_configuration.QueueName, false, consumer);

		Logger.LogInformation(
			$"Initializing subscription on queue '{_configuration.QueueName}' with ResourceKey '{_configuration.ResourceKey}' ...");
	}

	private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
	{
		IntegrationEvent? message;

		try
		{
			message = await _messageSerializer.DeserializeAsync<T>(Encoding.UTF8.GetString(eventArgs.Body.ToArray()), CancellationToken.None);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex,
				"an exception has occured while decoding queue message from Exchange '{ExchangeName}', message cannot be parsed. Error: {ExceptionMessage}",
				eventArgs.Exchange, ex.Message);
			await _channel.BasicRejectAsync(eventArgs.DeliveryTag, false);

			return;
		}

		if (message != null)
		{
			Logger.LogInformation($"Received message '{message.MessageId}' from Exchange '{_connectionFactory.ExchangeEventsName}', Queue '{_configuration.QueueName}'. Processing...");

			try
			{
				//TODO: provide valid cancellation token
				await ConsumeAsync((dynamic)message, CancellationToken.None);

				await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
			}
			catch (Exception ex)
			{
				await HandleConsumerExceptionAsync(ex, eventArgs, _channel, message, false);
			}
		}
	}

	private async Task HandleConsumerExceptionAsync(Exception ex, BasicDeliverEventArgs deliveryProps, IChannel channel,
		IMessage message, bool requeue)
	{
		Logger.LogWarning(ex,
			$"An error has occurred while processing Message '{message.MessageId}' from Exchange '{deliveryProps.Exchange}' : {ex.Message}. {(requeue ? "Reenqueuing..." : "Nacking...")}");

		if (!requeue)
		{
			await channel.BasicRejectAsync(deliveryProps.DeliveryTag, false);
		}
		else
		{
			await channel.BasicAckAsync(deliveryProps.DeliveryTag, false);
			// channel.BasicPublish(_configuration.QueueName, deliveryProps.RoutingKey, deliveryProps.BasicProperties,
			// 	deliveryProps.Body);
		}
	}

	private void GetQueueName(ConsumerConfiguration configuration)
	{
		if (string.IsNullOrWhiteSpace(configuration.ResourceKey))
			configuration.ResourceKey = typeof(T).Name;

		configuration.QueueName = typeof(T).Name;

		if (!string.IsNullOrWhiteSpace(configuration.QueueName))
			return;

		configuration.QueueName = GetType().Name;
		if (configuration.QueueName.EndsWith("Consumer", StringComparison.InvariantCultureIgnoreCase))
			configuration.QueueName =
				configuration.QueueName[..^"Consumer".Length];
	}

	#region Dispose

	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}

	#endregion
}