using BrewUp.Sales.Domain.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Sales.Domain;

public static class DomainHelper
{
    public static IServiceCollection AddSalesDomain(this IServiceCollection services)
    {
        services.AddScoped<ISalesDomainService, SalesDomainService>();

        services.AddCommandHandler<CreateSalesOrderCommandHandler>();
        
        return services;
    }
}