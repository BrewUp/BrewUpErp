using BrewUp.Dashboards.Entities.Dtos;
using BrewUp.Dashboards.Infrastructure.Repository;
using BrewUp.Dashboards.ReadModel;
using BrewUp.Dashboards.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Dashboards.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        services.AddDbContext<DashboardsContext>(options =>
            options.UseSqlServer(configurationManager["BrewUp:SqlServer:ConnectionString"]!));

        services.AddScoped<IDashboardsRepository<SalesByCustomers>, SalesByCustomersRepository>();
        
        return services;
    }
}