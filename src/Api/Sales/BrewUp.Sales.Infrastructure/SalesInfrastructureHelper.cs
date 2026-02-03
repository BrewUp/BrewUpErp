using BrewUp.Sales.Infrastructure.MongoDb;
using BrewUp.Sales.ReadModel.Dtos;
using BrewUp.Shared.Configuration;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Eventstore.gRPC;

namespace BrewUp.Sales.Infrastructure;

public static class SalesInfrastructureHelper
{
    public static IServiceCollection AddSalesInfrastructure(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        MongoDbSettings mongoDbSettings = new();
        configurationManager.GetSection("BrewUp:MongoDbSettings").Bind(mongoDbSettings);
        services.AddSalesMongoDb(mongoDbSettings);

        EventStoreSettings eventStoreSettings = new();
        configurationManager.GetSection("BrewUp:EventStore").Bind(eventStoreSettings);
        services.AddMufloneEventStore(eventStoreSettings.ConnectionString);
        
        services.AddKeyedScoped<IPersister, SalesPersister>("sales");
        services.AddScoped<IQueries<SalesOrder>, SalesOrderQueries>();

        return services;
    }
}