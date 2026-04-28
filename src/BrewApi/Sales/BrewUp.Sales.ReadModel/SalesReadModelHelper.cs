using BrewUp.Sales.ReadModel.Dtos;
using BrewUp.Sales.ReadModel.EventHandlers;
using BrewUp.Sales.ReadModel.Queries;
using BrewUp.Sales.ReadModel.Services;
using BrewUp.Shared.ReadModel;
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
        
        services.AddScoped<IQueries<SalesOrder>, SalesOrderQueries>();

        services.AddDomainEventHandler<SalesOrderCreatedEventHandler>();
        services.AddDomainEventHandler<SalesOrderCreatedForIntegrationEventHandler>();
        services.AddDomainEventHandler<SalesOrderCreatedWithPriceForIntegrationEventHandler>();
        
        return services;
    }
}