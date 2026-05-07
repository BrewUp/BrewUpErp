using BrewUp.Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Transport.RabbitMQ;
using Muflone.Transport.RabbitMQ.Models;

namespace BrewUp.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        ILoggerFactory loggerFactory,
        IConfigurationManager configurationManager)
    {

        RabbitMqSettings rabbitMqSettings = new();
        configurationManager.GetSection("BrewUp:RabbitMQ").Bind(rabbitMqSettings);

        //RabbitMQConfiguration rabbitMqConfiguration = new(rabbitMqSettings.Host,
        //    rabbitMqSettings.Username,
        //    rabbitMqSettings.Password,
        //    rabbitMqSettings.ExchangeCommandName,
        //    rabbitMqSettings.ExchangeEventName,
        //    rabbitMqSettings.ClientId);


        RabbitMQConfiguration rabbitMqConfiguration = new(
            Environment.GetEnvironmentVariable("RABBITMQ_HOST")!,
            Environment.GetEnvironmentVariable("RABBITMQ_USERNAME")!,
            Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")!,
            rabbitMqSettings.ExchangeCommandName,
            rabbitMqSettings.ExchangeEventName,
            rabbitMqSettings.ClientId);


        services.AddMufloneTransportRabbitMQ(loggerFactory, rabbitMqConfiguration);

        return services;
    }
}