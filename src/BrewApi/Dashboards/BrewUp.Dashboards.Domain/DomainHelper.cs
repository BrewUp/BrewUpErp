using BrewUp.Dashboards.Domain.CommandHandlers;
using BrewUp.Dashboards.SharedKernel.Messages.Commands;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages.Commands;

namespace BrewUp.Dashboards.Domain;

public static class DomainHelper
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<IDashboardsDomainService, DashboardsDomainService>();

        services.AddScoped<ICommandHandlerAsync<CreateSummaryByCustomer>, CreateSalesByCustomersCommandHandler>();
        services.AddScoped<ICommandHandlerAsync<IncreaseSalesSummaryByCustomer>, IncreaseSalesByCustomersCommandHandler>();
        
        return services;
    }
}