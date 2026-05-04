using BrewUp.Sagas.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Sagas.Facade;

public static class SagasFacadeHelper
{
    public static IServiceCollection AddSagas(this IServiceCollection services)
    {
        services.AddScoped<ISagasFacade, SagasFacade>();

        services.AddDomain();
        
        return services;
    }
}