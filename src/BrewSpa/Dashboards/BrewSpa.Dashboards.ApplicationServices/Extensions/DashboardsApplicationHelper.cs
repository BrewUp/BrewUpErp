using BrewSpa.Dashboards.ApplicationServices.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BrewSpa.Dashboards.ApplicationServices.Extensions;

public static class DashboardsApplicationHelper
{
    public static IServiceCollection AddDashboardsApplicationServices(this IServiceCollection services,
        WebAssemblyHostConfiguration configurationManager)
    {
        services.AddHttpClient<IDashboardService, DashboardService>(client =>
        {
            client.BaseAddress = new Uri(configurationManager["BrewApi:DashboardsApiBaseAddress"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        return services;
    }
}
