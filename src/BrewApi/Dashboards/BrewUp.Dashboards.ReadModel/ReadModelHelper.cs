using BrewUp.Dashboards.Entities.Dtos;
using BrewUp.Dashboards.ReadModel.Queries;
using BrewUp.Shared.ReadModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Dashboards.ReadModel;

public static class ReadModelHelper
{
    public static IServiceCollection AddReadModel(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        services.AddDbContext<DashboardsContext>(options =>
            options.UseSqlServer(configurationManager["BrewUp:SqlServer:ConnectionString"]!));
        services.AddScoped<IQueries<SalesByCustomers>, SalesByCustomersQueries>();
        return services;
    }
}
