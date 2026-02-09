using ApexCharts;
using Microsoft.Extensions.DependencyInjection;

namespace BrewSpa.Dashboards.Facade;

public static class DashboardsHelper
{
    public static IServiceCollection AddDashboardsFacade(this IServiceCollection services)
    {
        services.AddApexCharts();
        
        return services;
    }
}