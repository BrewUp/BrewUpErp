using BrewUp.Dashboards.Entities.Dtos;
using BrewUp.Dashboards.Infrastructure.Repository;
using BrewUp.Dashboards.SharedKernel.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Dashboards.Infrastructure;

public static class InfrastructureHelper
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfigurationManager configurationManager)
    {
        // services.AddDbContext<DashboardsContext>(options =>
        //     options.UseSqlServer(configurationManager["BrewUp:SqlServer:ConnectionString"]!));

        services.AddScoped<IDashboardsRepository<SalesByCustomers>, SummaryByCustomersRepository>();
        services.AddScoped<IDashboardsRepository<SalesByProducts>, SummaryByProductsRepository>();
        services.AddScoped<IDashboardsRepository<MessagesReceived>, MessagesReceivedRepository>();
        
        services.AddScoped<IMessagesReceivedService, MessagesReceivedService>();
        
        return services;
    }
}