using BrewUp.Shared;
using BrewUp.Shared.Agents;
using BrewUp.Warehouse.Domain;
using BrewUp.Warehouse.Facade.Acl;
using BrewUp.Warehouse.Facade.Agents;
using BrewUp.Warehouse.Infrastructure;
using BrewUp.Warehouse.ReadModel;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Warehouse.Facade;

public static class WarehouseFacadeHelper
{
    public static IServiceCollection AddWarehouse(this IServiceCollection services)
    {
        services.AddShared();
        services.AddScoped<IWarehouseFacade, WarehouseFacade>();
        services.AddScoped<WarehouseAgent>();
        services.AddScoped<IAgent>(sp => sp.GetRequiredService<WarehouseAgent>());
        services.AddScoped<IAgentCardProvider, WarehouseAgentCardProvider>();

        services.AddInfrastructure();
        services.AddReadModel();
        services.AddDomain();

        services.AddIntegrationEventHandler<WarehouseCreatedEventHandler>();
        services.AddIntegrationEventHandler<SalesOrderCreatedIntegrationEventHandler>();
        services.AddIntegrationEventHandler<BeerCreatedEventHandler>();
        services.AddIntegrationEventHandler<RequestBeerAvailablityRaisedEventHandler>();

        return services;
    }
}
