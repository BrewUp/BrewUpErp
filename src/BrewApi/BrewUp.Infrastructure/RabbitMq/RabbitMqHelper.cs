using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Transport.RabbitMQ;
using Muflone.Transport.RabbitMQ.Models;

namespace BrewUp.Infrastructure.RabbitMq;

public static class RabbitMqHelper
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services,
        ILoggerFactory loggerFactory,
        IConfigurationManager configurationManager)
    {
        RabbitMqSettings rabbitMqSettings = configurationManager.GetSection("BrewUp:RabbitMQ").Get<RabbitMqSettings>()
                                            ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:RabbitMQ'.");

        if (!rabbitMqSettings.UseRMQ) 
            return services;
        
        RabbitMQConfiguration rabbitMqConfiguration = new(rabbitMqSettings.Host,
            rabbitMqSettings.Username,
            rabbitMqSettings.Password,
            rabbitMqSettings.ExchangeCommandName,
            rabbitMqSettings.ExchangeEventName,
            rabbitMqSettings.ClientId);
        services.AddMufloneTransportRabbitMQ(loggerFactory, rabbitMqConfiguration);

        return services;
    }
}