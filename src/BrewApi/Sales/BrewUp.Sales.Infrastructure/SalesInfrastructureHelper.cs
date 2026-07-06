using BrewUp.Sales.Infrastructure.MongoDb;
using BrewUp.Shared.Configuration;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Sales.Infrastructure;

public static class SalesInfrastructureHelper
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        MongoDbSettings mongoDbSettings = configurationManager.GetSection("BrewUp:MongoDbSettings").Get<MongoDbSettings>()
                                          ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:MongoDbSettings'.");
        services.AddSalesMongoDb(mongoDbSettings);

        services.AddKeyedScoped<IPersister, SalesPersister>("sales");

        return services;
    }
}