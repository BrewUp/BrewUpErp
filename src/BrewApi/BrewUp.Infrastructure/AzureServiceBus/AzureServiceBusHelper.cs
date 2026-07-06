using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Transport.Azure;
using Muflone.Transport.Azure.Models;

namespace BrewUp.Infrastructure.AzureServiceBus;

public static class AzureServiceBusHelper
{
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        AzureServiceBusSettings azureServiceBusSettings = configuration.GetSection("BrewUp:AzureServiceBus").Get<AzureServiceBusSettings>()
                                                          ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:AzureServiceBus'.");

        if (!azureServiceBusSettings.UseAzureServiceBus) 
            return services;
        
        AzureServiceBusConfiguration azureServiceBusConfiguration = new(
            azureServiceBusSettings.ConnectionString,
            azureServiceBusSettings.TopicName,
            azureServiceBusSettings.ClientId,
            azureServiceBusSettings.MaxConcurrentCalls
        );
        services.AddMufloneTransportAzure(azureServiceBusConfiguration);

        return services;
    }
}