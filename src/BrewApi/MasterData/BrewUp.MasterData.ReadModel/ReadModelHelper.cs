using BrewUp.MasterData.ReadModel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.MasterData.ReadModel;

public static class ReadModelHelper
{
    public static IServiceCollection AddMasterDataReadModel(this IServiceCollection services)
    {
        services.AddScoped<IMasterDataQueryService, MasterDataQueryService>();
        
        return services;
    }
}