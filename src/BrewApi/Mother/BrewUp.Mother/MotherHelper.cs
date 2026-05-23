using BrewUp.Mother.Agents;
using BrewUp.Mother.Clients;
using BrewUp.Mother.RabbitMq;
using Muflone;
using Muflone.Transport.RabbitMQ;
using Muflone.Transport.RabbitMQ.Models;

namespace BrewUp.Mother;

public static class MotherHelper
{
    public static IServiceCollection AddMother(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        services.AddLogging();
        using var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        
        RabbitMqSettings rabbitMqSettings = new();
        configurationManager.GetSection("RabbitMQ").Bind(rabbitMqSettings);
        
        RabbitMQConfiguration rabbitMqConfiguration = new(rabbitMqSettings.Host,
            rabbitMqSettings.Username,
            rabbitMqSettings.Password,
            rabbitMqSettings.ExchangeCommandName,
            rabbitMqSettings.ExchangeEventName,
            rabbitMqSettings.ClientId);
        services.AddMufloneTransportRabbitMQ(loggerFactory, rabbitMqConfiguration);
        
        services.AddHttpClient("mcp");
        services.AddScoped<IMcpToolClient, McpToolClient>();
        services.AddScoped<IRecommendationWriter, RecommendationWriter>();
        
        services.AddIntegrationEventHandler<InventoryRiskAgent>();
        
        return services;
    }
}