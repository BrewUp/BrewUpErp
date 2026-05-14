using Microsoft.Extensions.Logging;

namespace Muflone.Transport.RabbitMQ.Consumers;

public abstract class ConsumerBase
{
	protected readonly ILogger Logger;

	protected ConsumerBase(ILoggerFactory loggerFactory)
	{
		Logger = loggerFactory.CreateLogger(GetType());
	}
}