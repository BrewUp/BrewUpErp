using Azure;
using Azure.AI.OpenAI;
using BrewUp.AI.Facade.Chat;
using BrewUp.AI.Facade.MasterData;
using BrewUp.AI.Facade.Sales;
using BrewUp.AI.Facade.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.AI.Facade;

public static class BrewUpAiHelper
{
    public static IServiceCollection AddBrewUpAi(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<BrewUpAiTools>();
        
        var options = configuration
                          .GetSection(AzureOpenAiOptions.SectionName)
                          .Get<AzureOpenAiOptions>()
                      ?? throw new InvalidOperationException(
                          "Missing AzureOpenAI configuration section.");

        services.AddSingleton(options);

        // services.AddSingleton<IChatClient>(_ =>
        // {
        //     var client = new OpenAIClient(
        //         apiKey: options.ApiKey);
        //
        //     return client
        //         .GetChatClient(options.DeploymentName)
        //         .AsIChatClient();
        // });
        
        services.AddSingleton<IChatClient>(_ =>
        {
            var azureClient = new AzureOpenAIClient(
                new Uri(options.Endpoint),
                new AzureKeyCredential(options.ApiKey));

            return azureClient
                .GetChatClient(options.DeploymentName)
                .AsIChatClient()
                .AsBuilder()
                .UseFunctionInvocation()
                .Build();
        });

        services.AddScoped<BrewUpChatService>();
        services.AddScoped<IBeerCatalogQueriesFacade, BeerCatalogQueriesFacade>();
        services.AddScoped<ISalesQueriesFacade, SalesQueriesFacade>();
        
        return services;
    }
}