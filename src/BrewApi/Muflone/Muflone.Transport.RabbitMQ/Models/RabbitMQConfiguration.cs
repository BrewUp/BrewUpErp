namespace Muflone.Transport.RabbitMQ.Models;

public class RabbitMQConfiguration
{
	public readonly string HostName;
	public readonly string VirtualHost;
	public readonly string UserName;
	public readonly string Password;
	public readonly TimeSpan RetryDelay;
	public readonly string ExchangeCommandsName;
	public readonly string ExchangeEventsName;
	public readonly string ClientId;


	public RabbitMQConfiguration(string hostName, string userName, string password, string exchangeCommandsName,
		string exchangeEventsName, string clientId)
		: this(hostName, userName, password, TimeSpan.FromSeconds(30), exchangeCommandsName, exchangeEventsName, clientId)
	{
	}

	public RabbitMQConfiguration(string hostName, string userName, string password, TimeSpan retryDelay,
		string exchangeCommandsName, string exchangeEventsName, string clientId)
		: this(hostName, string.Empty, userName, password, retryDelay, exchangeCommandsName, exchangeEventsName, clientId)
	{
	}

	public RabbitMQConfiguration(string hostName, string vhost, string userName, string password, TimeSpan retryDelay,
		string exchangeCommandsName, string exchangeEventsName, string clientId)
	{
		HostName = hostName;
		UserName = userName;
		Password = password;

		RetryDelay = retryDelay;
		ExchangeCommandsName = exchangeCommandsName;
		ExchangeEventsName = exchangeEventsName;

		ClientId = clientId;

		VirtualHost = string.IsNullOrWhiteSpace(vhost) ? "/" : vhost;
	}
}