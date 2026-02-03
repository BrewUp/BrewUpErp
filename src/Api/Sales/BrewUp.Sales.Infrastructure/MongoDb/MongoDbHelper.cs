using BrewUp.Sales.Infrastructure.ReadModel;
using BrewUp.Shared.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Eventstore.gRPC.Persistence;

namespace BrewUp.Sales.Infrastructure.MongoDb;

public static class MongoDbHelper
{
    public static IServiceCollection AddSalesMongoDb(this IServiceCollection services,
        MongoDbSettings mongoDbSettings)
    {
        services.AddSingleton<IEventStorePositionRepository>(x =>
            new EventStorePositionRepository(x.GetRequiredService<ILogger<EventStorePositionRepository>>(), mongoDbSettings));

        return services;
    }
}