using BrewUp.Mother.Agents;
using BrewUp.Mother.Clients;
using Muflone;

namespace BrewUp.Mother;

public static class MotherHelper
{
    public static IServiceCollection AddMother(this IServiceCollection services)
    {
        services.AddScoped<IMcpToolClient, McpToolClient>();
        services.AddScoped<IRecommendationWriter, RecommendationWriter>();
        services.AddIntegrationEventHandler<InventoryRiskAgent>();
        
        return services;
    }
}