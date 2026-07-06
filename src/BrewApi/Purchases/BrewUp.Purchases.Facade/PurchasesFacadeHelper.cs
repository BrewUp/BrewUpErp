using BrewUp.Purchases.Domain;
using BrewUp.Purchases.Facade.Acl;
using BrewUp.Purchases.Infrastructure;
using BrewUp.Purchases.ReadModel;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Purchases.Facade;

public static class PurchasesFacadeHelper
{
    public static IServiceCollection AddPurchases(this IServiceCollection services)
    {
        services.AddScoped<IPurchasesFacade, PurchasesFacade>();
        
        services.AddDomain();
        services.AddReadModel();
        services.AddInfrastructure();
        
        services.AddIntegrationEventHandler<SupplierCreatedEventHandler>();
        services.AddIntegrationEventHandler<BeerCreatedEventHandler>();
        
        return services;
    }
}