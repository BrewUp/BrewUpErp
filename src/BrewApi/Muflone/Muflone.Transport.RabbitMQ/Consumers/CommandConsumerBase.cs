using Microsoft.Extensions.Logging;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Persistence;
using Muflone.Transport.RabbitMQ.Abstracts;
using Muflone.Transport.RabbitMQ.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Muflone.Transport.RabbitMQ.Consumers;

public abstract class CommandConsumerBase<T> : ConsumerBase, ICommandConsumer<T>, IAsyncDisposable
	where T : Command
{
	private readonly ISerializer _messageSerializer;
	private readonly ConsumerConfiguration _configuration;
	private readonly IRabbitMQConnectionFactory _connectionFactory;
	private IChannel _channel;

	protected abstract ICommandHandlerAsync<T> HandlerAsync { get; }

	/// <summary>
	/// For now just as a proxy to pass directly to the Handler this class is wrapping
	/// </summary>
	protected IRepository Repository { get; }

	protected CommandConsumerBase(IRepository repository, IRabbitMQConnectionFactory connectionFactory,
		ILoggerFactory loggerFactory)
		: this(new ConsumerConfiguration(), repository, connectionFactory, loggerFactory)
	{
	}

	protected CommandConsumerBase(ConsumerConfiguration configuration, IRepository repository,
		IRabbitMQConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
		: base(loggerFactory)
	{
		Repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
		_messageSerializer = new Serializer();
		_channel = null!;

		GetQueueName(configuration);
		_configuration = configuration;
	}

	public async Task ConsumeAsync(T message, CancellationToken cancellationToken = default)
	{
		await HandlerAsync.HandleAsync(message, cancellationToken);
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
		await InitExchangesAsync();

		_channel = await _connectionFactory.CreateChannelAsync();

		await _channel.ExchangeDeclareAsync(_connectionFactory.ExchangeCommandsName, ExchangeType.Direct, true);
		await _channel.QueueDeclareAsync(_configuration.QueueName, true, false, false);
		await _channel.QueueBindAsync(_configuration.QueueName, _connectionFactory.ExchangeCommandsName,
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

	private async Task OnChannelExceptionAsync(CallbackExceptionEventArgs e)
	{
		Logger.LogError(e.Exception, $"RabbitMQ Channel has encountered an error: {e.Exception.Message}");

		await InitChannelAsync();
		await InitSubscriptionAsync();
	}

	private async Task InitSubscriptionAsync()
	{
		Logger.LogInformation($"Initializing subscription on queue '{_configuration.QueueName}' ...");

		var consumer = new AsyncEventingBasicConsumer(_channel);
		consumer.ReceivedAsync += OnMessageReceivedAsync;
		await _channel.BasicConsumeAsync(_configuration.QueueName, false, consumer);
	}

	private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
	{
		Command? command;

		try
		{
			command = await _messageSerializer.DeserializeAsync<T>(Encoding.UTF8.GetString(eventArgs.Body.ToArray()), CancellationToken.None);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "An exception has occured while decoding queue message from Exchange '{ExchangeName}', message cannot be parsed. Error: {ExceptionMessage}", eventArgs.Exchange, ex.Message);
			await _channel.BasicRejectAsync(eventArgs.DeliveryTag, false);

			return;
		}

		if (command != null)
		{
			Logger.LogInformation($"Received message '{command.MessageId}' from Exchange '{_connectionFactory.ExchangeCommandsName}', Queue '{_configuration.QueueName}'. Processing...");
			try
			{
				//TODO: provide valid cancellation token
				await ConsumeAsync((dynamic)command, CancellationToken.None);

				await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
			}
			catch (Exception ex)
			{
				await HandleConsumerExceptionAsync(ex, eventArgs, _channel, command, false);
			}
		}
	}

	private async Task HandleConsumerExceptionAsync(Exception ex, BasicDeliverEventArgs deliveryProps, IChannel channel,
		IMessage message, bool requeue)
	{
		var errorMsg = $"An error has occurred while processing Message '{message.MessageId}' from Exchange '{deliveryProps.Exchange}' : {ex.Message} . {(requeue ? "Reenqueuing..." : "Nacking...")}";
		Logger.LogWarning(ex, errorMsg);

		if (!requeue)
		{
			await channel.BasicRejectAsync(deliveryProps.DeliveryTag, false);
		}
		else
		{
			await channel.BasicAckAsync(deliveryProps.DeliveryTag, false);
			// await channel.BasicPublishAsync(_configuration.QueueName, deliveryProps.RoutingKey, false,
			// 	deliveryProps.BasicProperties, deliveryProps.Body).ConfigureAwait(false);
		}
	}

	private async Task InitExchangesAsync()
	{
		await _channel.ExchangeDeclareAsync(_connectionFactory.ExchangeCommandsName, ExchangeType.Direct, durable: true);
	}

	private void GetQueueName(ConsumerConfiguration configuration)
	{
		configuration.QueueName = typeof(T).Name;

		if (string.IsNullOrWhiteSpace(configuration.ResourceKey))
			configuration.ResourceKey = typeof(T).Name;

		if (!string.IsNullOrWhiteSpace(configuration.QueueName))
			return;

		configuration.QueueName = GetType().Name;
		if (configuration.QueueName.EndsWith("Consumer", StringComparison.InvariantCultureIgnoreCase))
			configuration.QueueName =
				configuration.QueueName[..^"Consumer".Length];
	}

	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}
}