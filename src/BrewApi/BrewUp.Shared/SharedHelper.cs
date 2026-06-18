using BrewUp.Shared.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrewUp.Shared;

public static class SharedHelper
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.TryAddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build());
        services.AddScoped<IMcpToolClient, McpToolClient>();

        return services;
    }
}
