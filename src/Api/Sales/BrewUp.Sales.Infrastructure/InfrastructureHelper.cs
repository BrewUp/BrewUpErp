using BrewUp.Sales.Infrastructure.MongoDb;
using BrewUp.Sales.Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Eventstore.gRPC;
using Muflone.Transport.RabbitMQ;
using Muflone.Transport.RabbitMQ.Models;

namespace BrewUp.Sales.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddSalesInfrastructure(this IServiceCollection services,
        ILoggerFactory loggerFactory,
        IConfigurationManager configurationManager)
    {
        MongoDbSettings mongoDbSettings = new();
        configurationManager.GetSection("Blazar:MongoDbSettings").Bind(mongoDbSettings);
        services.AddMongoDb(mongoDbSettings);

        EventStoreSettings eventStoreSettings = new();
        configurationManager.GetSection("Blazar:EventStore").Bind(eventStoreSettings);
        services.AddMufloneEventStore(eventStoreSettings.ConnectionString);
        
        RabbitMqSettings rabbitMqSettings = new();
        configurationManager.GetSection("Blazar:RabbitMQ").Bind(rabbitMqSettings);
        
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