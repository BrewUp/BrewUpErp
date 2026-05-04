using BrewUp.Sagas.Domain.CommandHandlers;
using BrewUp.Shared.Messages.Commands.Sagas;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages.Commands;

namespace BrewUp.Sagas.Domain;

public static class SagasDomainHelper
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandlerAsync<PlaceSalesOrder>, PlaceSalesOrderCommandHandler>();
        
        return services;
    }
}