using BrewSpa.MasterData.Application.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BrewSpa.MasterData.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMasterDataServices(this IServiceCollection services, 
        WebAssemblyHostConfiguration configurationManager)
    {
        services.AddHttpClient<ICustomerService, CustomerService>(client =>
        {
            client.BaseAddress = new Uri(configurationManager["BrewApi:MasterDataApiBaseAddress"]!); 
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        return services;
    }
}
