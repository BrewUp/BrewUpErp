using BrewUp.Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Transport.RabbitMQ;
using Muflone.Transport.RabbitMQ.Models;

namespace BrewUp.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ILoggerFactory loggerFactory,
        IConfigurationManager configurationManager)
    {
        RabbitMqSettings rabbitMqSettings = new();

        configurationManager
            .GetSection("BrewUp:RabbitMQ")
            .Bind(rabbitMqSettings);

        var host =
            configurationManager["RABBITMQ_HOST"]
            ?? throw new InvalidOperationException("Missing RABBITMQ_HOST");

        var username =
            configurationManager["RABBITMQ_USERNAME"]
            ?? throw new InvalidOperationException("Missing RABBITMQ_USERNAME");

        var password =
            configurationManager["RABBITMQ_PASSWORD"]
            ?? throw new InvalidOperationException("Missing RABBITMQ_PASSWORD");

        var rabbitMqConfiguration = new RabbitMQConfiguration(
            host,
            username,
            password,
            rabbitMqSettings.ExchangeCommandName,
            rabbitMqSettings.ExchangeEventName,
            rabbitMqSettings.ClientId);

        services.AddMufloneTransportRabbitMQ(
            loggerFactory,
            rabbitMqConfiguration);

        return services;
    }
}