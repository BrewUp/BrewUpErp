using BrewUp.Shared.Configuration;
using MongoDB.Driver;

namespace BrewUp.AI.McpServer;

public static class MongoDbHelper
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services,
        MongoDbSettings mongoDbSettings)
    {
        services.AddSingleton<IMongoClient>(new MongoClient(mongoDbSettings.ConnectionString));

        return services;
    }
}