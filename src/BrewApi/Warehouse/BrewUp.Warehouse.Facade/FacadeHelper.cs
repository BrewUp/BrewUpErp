using BrewUp.Warehouse.Facade.Acl;
using BrewUp.Warehouse.Infrastructure;
using BrewUp.Warehouse.ReadModel;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Warehouse.Facade;

public static class FacadeHelper
{
    public static IServiceCollection AddWarehouse(this IServiceCollection services)
    {
        services.AddInfrastructure();
        services.AddReadModel();

        services.AddIntegrationEventHandler<WarehouseCreatedEventHandler>();
        services.AddIntegrationEventHandler<SalesOrderCreatedIntegrationEventHandler>();
        
        return services;
    }
}