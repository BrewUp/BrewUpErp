using BrewUp.Shared.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Shared;

public static class SharedHelper
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddScoped<IMcpToolClient, McpToolClient>();

        return services;
    }
}
