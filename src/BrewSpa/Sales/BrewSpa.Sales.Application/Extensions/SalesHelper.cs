using BrewSpa.Sales.Application.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BrewSpa.Sales.Application.Extensions;

public static class SalesHelper
{
    public static IServiceCollection AddSalesServices(this IServiceCollection services, 
        WebAssemblyHostConfiguration configurationManager)
    {
        services.AddHttpClient("SalesApi", client =>
        {
            client.BaseAddress = new Uri(configurationManager["BrewApi:SalesApiBaseAddress"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("SagaApi", client =>
        {
            client.BaseAddress = new Uri(configurationManager["BrewApi:SagaApiBaseAddress"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<ISalesService>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var salesHttpClient = httpClientFactory.CreateClient("SalesApi");
            var sagaHttpClient = httpClientFactory.CreateClient("SagaApi");
            return new SalesService(salesHttpClient, sagaHttpClient);
        });

        return services;
    }
}
