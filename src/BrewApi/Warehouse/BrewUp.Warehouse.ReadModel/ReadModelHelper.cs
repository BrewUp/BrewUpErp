using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.ReadModel.Dtos;
using BrewUp.Warehouse.ReadModel.EventHandlers;
using BrewUp.Warehouse.ReadModel.Queries;
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
        
        services.AddScoped<IQueries<Shipment>, ShipmentQueries>();

        services.AddDomainEventHandler<ShipmentPendingForPreparationEventHandler>();
        
        return services;
    }
}