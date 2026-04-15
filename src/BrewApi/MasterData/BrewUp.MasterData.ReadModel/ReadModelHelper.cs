using BrewUp.MasterData.ReadModel.Dtos;
using BrewUp.MasterData.ReadModel.Queries;
using BrewUp.MasterData.ReadModel.Services;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.MasterData.ReadModel;

public static class ReadModelHelper
{
    public static IServiceCollection AddMasterDataReadModel(this IServiceCollection services)
    {
        services.AddScoped<IQueries<Customer>, CustomerQueries>();
        services.AddScoped<IQueries<Warehouse>, WarehouseQueries>();
        services.AddScoped<ICustomerQueryService, CustomerQueryService>();
        services.AddScoped<IWarehouseQueryService, WarehouseQueryService>();
        
        return services;
    }
}