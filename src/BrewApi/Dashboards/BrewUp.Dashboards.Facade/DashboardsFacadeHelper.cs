using BrewUp.Dashboards.Facade.Acl;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.Dashboards.Facade;

public static class DashboardsFacadeHelper
{
    public static IServiceCollection AddDashboards(this IServiceCollection services)
    {
        services.AddIntegrationEventHandler<SalesOrderCreatedIntegrationForBeerSummaryEventHandler>();
        services.AddIntegrationEventHandler<SalesOrderCreatedIntegrationForCustomerSummaryEventHandler>();
        
        return services;
    }
}