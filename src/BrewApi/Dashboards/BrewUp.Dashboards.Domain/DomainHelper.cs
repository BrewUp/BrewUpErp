using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Dashboards.Domain;

public static class DomainHelper
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<IDashboardsDomainService, DashboardsDomainService>();
        
        return services;
    }
}