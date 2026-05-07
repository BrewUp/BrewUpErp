using BrewUp.Warehouse.Domain.CommandHandlers;
using BrewUp.Warehouse.Domain.Services;
using BrewUp.Warehouse.SharedKernel.Messages.Commands;
using Microsoft.Extensions.DependencyInjection;
using Muflone;
using Muflone.Messages.Commands;

namespace BrewUp.Warehouse.Domain;

public static class DomainHelper
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<IWarehouseDomainService, WarehouseDomainService>();

        services.AddCommandHandler<PrepareShipmentCommandHandler>();
        //services.AddCommandHandler<RequestBeersAvailabilityCommandHandler>();

        services.AddScoped<ICommandHandlerAsync<AddItemStocks>, AddItemStocksCommandHandlerAsync>();
        
        return services;
    }
}