using BrewUp.Sales.ReadModel.Acl;
using BrewUp.Sales.ReadModel.EventHandlers;
using BrewUp.Sales.ReadModel.Services;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Sales.ReadModel;

public static class SalesReadModelHelper
{
    public static IServiceCollection AddSalesReadModel(this IServiceCollection services)
    {
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBeerService, BeerService>();

        services.AddDomainEventHandler<SalesOrderCreatedEventHandler>();
        services.AddIntegrationEventHandler<CustomerCreatedEventHandler>();
        services.AddIntegrationEventHandler<CustomerUpdatedEventHandler>();
        services.AddIntegrationEventHandler<CustomerDeletedEventHandler>();

        services.AddIntegrationEventHandler<BeerCreatedEventHandler>();

        return services;
    }
}