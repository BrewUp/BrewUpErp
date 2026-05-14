using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Purchases.Domain;

public static class DomainHelper
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services; 
    }
}