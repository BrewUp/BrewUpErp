using BrewUp.Warehouse.ReadModel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Warehouse.ReadModel;

public static class ReadModelHelper
{
    public static IServiceCollection AddReadModel(this IServiceCollection services)
    {
        services.AddScoped<IWarehouseService, WarehouseService>();
        
        return services;
    }
}