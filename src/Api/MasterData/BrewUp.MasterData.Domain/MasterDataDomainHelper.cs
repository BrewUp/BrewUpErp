using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.MasterData.Domain;

public static class MasterDataDomainHelper
{
    public static IServiceCollection AddMasterDataDomain(this IServiceCollection services)
    {
        services.AddScoped<IMasterDataDomainService, MasterDataDomainService>();
        
        return services;
    }
}