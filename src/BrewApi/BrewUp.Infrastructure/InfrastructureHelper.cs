using BrewUp.Infrastructure.AzureServiceBus;
using BrewUp.Infrastructure.MongoDb;
using BrewUp.Infrastructure.RabbitMq;
using BrewUp.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Eventstore.gRPC;

namespace BrewUp.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        ILoggerFactory loggerFactory,
        IConfigurationManager configurationManager)
    {
        MongoDbSettings mongoDbSettings = configurationManager.GetSection("BrewUp:MongoDbSettings").Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:MongoDbSettings'.");
        services.AddMongoDb(mongoDbSettings);
        
        services.AddRabbitMq(loggerFactory, configurationManager);
        services.AddAzureServiceBus(configurationManager);

        EventStoreSettings eventStoreSettings = new();
        configurationManager.GetSection("BrewUp:EventStore").Bind(eventStoreSettings);
        services.AddMufloneEventStore(eventStoreSettings.ConnectionString);

        services.AddAntiforgery();

        return services;
    }
}