using BrewUp.Warehouse.ReadModel.EventHandlers;
using BrewUp.Warehouse.ReadModel.Services;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Warehouse.ReadModel;

public static class ReadModelHelper
{
    public static IServiceCollection AddReadModel(this IServiceCollection services)
    {
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IShipmentService, ShipmentService>();

        services.AddDomainEventHandler<ShipmentPendingForPreparationEventHandler>();
        
        return services;
    }
}