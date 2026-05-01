using BrewUp.Dashboards.Entities.Dtos;
using BrewUp.Dashboards.ReadModel.Queries;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Dashboards.ReadModel;

public static class ReadModelHelper
{
    public static IServiceCollection AddReadModel(this IServiceCollection services)
    {
        services.AddScoped<IQueries<SalesByCustomers>, SalesByCustomersQueries>();
        
        return services;
    }
}