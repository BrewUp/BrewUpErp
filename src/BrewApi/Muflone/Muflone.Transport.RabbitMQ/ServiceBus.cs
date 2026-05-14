using Microsoft.Extensions.Logging;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.Persistence;
using Muflone.Transport.RabbitMQ.Abstracts;
using Polly;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

namespace Muflone.Transport.RabbitMQ;

public class ServiceBus : IServiceBus, IEventBus
{
	private const string TraceParentKey = "traceparent";
	private const string TraceStateKey = "tracestate";
	private static readonly ActivitySource ServiceBusActivitySource = new("Muflone.ServiceBus", "1.0.0");
	private static readonly ActivitySource EventBusActivitySource = new("Muflone.EventBus", "1.0.0");

	private readonly IRabbitMQConnectionFactory _connectionFactory;
	private readonly ISerializer _messageSerializer;
	private readonly ILogger _logger;
	private Task<IChannel>? _writerChannelTask;

	public ServiceBus(IRabbitMQConnectionFactory mufloneConnectionFactory, ILoggerFactory loggerFactory,
			ISerializer? messageSerializer = null)
	{
		_connectionFactory = mufloneConnectionFactory
												 ?? throw new ArgumentNullException(nameof(mufloneConnectionFactory));
		_messageSerializer = messageSerializer
												 ?? new Serializer();
		_logger = loggerFactory.CreateLogger(GetType())
							?? throw new ArgumentNullException(nameof(loggerFactory));
	}

	public async Task SendAsync<T>(T request, CancellationToken cancellationToken = default)
			where T : class, ICommand
	{
		request.UserProperties ??= new Dictionary<string, object>();

		using var fallbackActivity = StartFallbackProducerActivity(
				ServiceBusActivitySource,
				$"{typeof(T).Name} send",
				request);

		fallbackActivity?.SetTag("messaging.operation", "send");
		fallbackActivity?.SetTag("messaging.message.type", typeof(T).Name);
		fallbackActivity?.SetTag("messaging.message.id", request.MessageId.ToString());

		var policy = Policy.Handle<Exception>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
				{
					_logger.LogWarning(ex,
									"Could not send message '{MessageId}' to Exchange '{ExchangeName}', after {Timeout}s : {ExceptionMessage}",
									request,
									_connectionFactory.ExchangeCommandsName,
									$"{time.TotalSeconds:n1}", ex.Message);
				});

		var properties = new BasicProperties
		{
			Persistent = true,
			Headers = new Dictionary<string, object?>
						{
								{ "message-type", request.GetType().FullName! }
						}
		};

		InjectTraceHeaders(properties.Headers!, request.UserProperties);

		var serializedMessage = await _messageSerializer.SerializeAsync(request, cancellationToken);

		await policy.ExecuteAsync(async () =>
		{
			var channel = await GetWriterChannelAsync();
			await channel.BasicPublishAsync(
							_connectionFactory.ExchangeCommandsName,
							request.GetType().Name,
							true,
							properties,
							Encoding.UTF8.GetBytes(serializedMessage),
							cancellationToken: cancellationToken);

			_logger.LogInformation(
							$"message '{request}' sent to Exchange '{_connectionFactory.ExchangeCommandsName}'",
							request,
							_connectionFactory.ExchangeCommandsName);
		});
	}

	public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
			where T : class, IEvent
	{
		message.UserProperties ??= new Dictionary<string, object>();

		using var fallbackActivity = StartFallbackProducerActivity(
				EventBusActivitySource,
				$"{typeof(T).Name} publish",
				message);

		fallbackActivity?.SetTag("messaging.operation", "publish");
		fallbackActivity?.SetTag("messaging.message.type", typeof(T).Name);
		fallbackActivity?.SetTag("messaging.message.id", message.MessageId.ToString());

		var policy = Policy.Handle<Exception>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
				{
					_logger.LogWarning(ex,
									"Could not publish message '{MessageId}' to Exchange '{ExchangeName}', after {Timeout}s : {ExceptionMessage}",
									message,
									_connectionFactory.ExchangeEventsName,
									$"{time.TotalSeconds:n1}", ex.Message);
				});

		var properties = new BasicProperties
		{
			Persistent = true,
			Headers = new Dictionary<string, object?>
						{
								{ "message-type", message.GetType().FullName! }
						}
		};

		InjectTraceHeaders(properties.Headers!, message.UserProperties);

		var serializedMessage = await _messageSerializer.SerializeAsync(message, cancellationToken).ConfigureAwait(false);

		await policy.ExecuteAsync(async () =>
				{
					var channel = await GetWriterChannelAsync().ConfigureAwait(false);
					await channel.BasicPublishAsync(
											_connectionFactory.ExchangeEventsName,
											message.GetType().Name,
											true,
											properties,
											Encoding.UTF8.GetBytes(serializedMessage),
											cancellationToken: cancellationToken)
									.ConfigureAwait(false);

					_logger.LogInformation(
									$"message '{message}' published to Exchange '{_connectionFactory.ExchangeEventsName}'");
				})
				.ConfigureAwait(false);
	}

	private static void InjectTraceHeaders(IDictionary<string, object?> headers, IDictionary<string, object> userProperties)
	{
		var traceParent = Activity.Current?.Id;
		var traceState = Activity.Current?.TraceStateString;

		if (string.IsNullOrWhiteSpace(traceParent) &&
				userProperties.TryGetValue(TraceParentKey, out var traceParentObj))
		{
			traceParent = traceParentObj?.ToString();
		}

		if (string.IsNullOrWhiteSpace(traceState) &&
				userProperties.TryGetValue(TraceStateKey, out var traceStateObj))
		{
			traceState = traceStateObj?.ToString();
		}

		if (!string.IsNullOrWhiteSpace(traceParent))
		{
			headers[TraceParentKey] = traceParent;
			userProperties[TraceParentKey] = traceParent;
		}

		if (!string.IsNullOrWhiteSpace(traceState))
		{
			headers[TraceStateKey] = traceState;
			userProperties[TraceStateKey] = traceState;
		}
	}

	private static Activity? StartFallbackProducerActivity(ActivitySource activitySource, string activityName, IMessage message)
	{
		if (Activity.Current != null)
			return null;

		if (!TryExtractParentContext(message.UserProperties, out var parentContext))
			return null;

		return activitySource.StartActivity(activityName, ActivityKind.Producer, parentContext);
	}

	private static bool TryExtractParentContext(IDictionary<string, object> userProperties, out ActivityContext parentContext)
	{
		parentContext = default;

		if (!userProperties.TryGetValue(TraceParentKey, out var traceParentObj))
			return false;

		var traceParent = traceParentObj?.ToString();
		if (string.IsNullOrWhiteSpace(traceParent))
			return false;

		var traceState = userProperties.TryGetValue(TraceStateKey, out var traceStateObj)
				? traceStateObj?.ToString()
				: null;

		return ActivityContext.TryParse(traceParent, traceState, out parentContext);
	}

	private Task<IChannel> GetWriterChannelAsync()
	{
		if (_writerChannelTask == null)
		{
			_writerChannelTask = _connectionFactory.CreateChannelAsync();
		}

		return _writerChannelTask;
	}

	#region Dispose

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			// _poller!.Stop();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	~ServiceBus()
	{
		Dispose(false);
	}

	#endregion
}
