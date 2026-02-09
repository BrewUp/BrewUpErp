using Blazor.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace BrewSpa.Shared.Components;

public static class SharedComponentsHelper
{
    public static IServiceCollection AddSharedComponents(this IServiceCollection services)
    {
        services.AddScoped<IMessagingService>(sp =>
        {
            var synchronizationContext = SynchronizationContext.Current;
            return new MessagingService(synchronizationContext, TimeSpan.FromSeconds(10));
        });
        return services;
    }
}