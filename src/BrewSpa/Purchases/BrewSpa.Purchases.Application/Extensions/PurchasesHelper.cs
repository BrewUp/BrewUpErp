using BrewSpa.Purchases.Application.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BrewSpa.Purchases.Application.Extensions;

public static class PurchasesHelper
{
    public static IServiceCollection AddPurchasesServices(this IServiceCollection services,
        WebAssemblyHostConfiguration configurationManager)
    {
        services.AddHttpClient<IPurchaseService, PurchaseService>(client =>
        {
            client.BaseAddress = new Uri(configurationManager["BrewApi:PurchasesApiBaseAddress"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
