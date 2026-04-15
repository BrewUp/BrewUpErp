using BrewUp.MasterData.ReadModel.Dtos;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.MasterData.Infrastructure;

public static class MasterDataInfrastructureHelper
{
    public static IServiceCollection AddMasterDataInfrastructure(this IServiceCollection services)
    {
        services.AddKeyedScoped<IPersister, MasterDataPersister>("masterdata");
        services.AddScoped<IQueries<Customer>, CustomerQueries>();

        return services;
    }
}