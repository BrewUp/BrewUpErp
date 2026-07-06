using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Purchases.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddKeyedScoped<IPersister, PurchasesPersister>("purchases");
        
        return services;
    }
}