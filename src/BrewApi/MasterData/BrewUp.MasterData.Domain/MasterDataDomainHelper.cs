using BrewUp.MasterData.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.MasterData.Domain;

public static class MasterDataDomainHelper
{
    public static IServiceCollection AddMasterDataDomain(this IServiceCollection services)
    {
        services.AddScoped<ICustomerDomainService, CustomerDomainService>();
        services.AddScoped<IBeerDomainService, BeerDomainService>();
        services.AddScoped<IWarehouseDomainService, WarehouseDomainService>();
        
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
        
        return services;
    }
}