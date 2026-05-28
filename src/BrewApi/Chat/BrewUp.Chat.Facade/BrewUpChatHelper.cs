using Azure;
using Azure.AI.OpenAI;
using BrewUp.Chat.Facade.Chat;
using BrewUp.Chat.Facade.MasterData;
using BrewUp.Chat.Facade.Mcp;
using BrewUp.Chat.Facade.Sales;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Chat.Facade;

public static class BrewUpChatHelper
{
    public static IServiceCollection AddBrewUpChat(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient();

        var mcpServerOptions = configuration
                                   .GetSection(McpServerOptions.SectionName)
                                   .Get<McpServerOptions>()
                               ?? throw new InvalidOperationException(
                                   "Missing BrewUp:McpServers configuration section.");
        services.AddSingleton(mcpServerOptions);
        
        var options = configuration
                          .GetSection(AzureOpenAiOptions.SectionName)
                          .Get<AzureOpenAiOptions>()
                      ?? throw new InvalidOperationException(
                          "Missing AzureOpenAI configuration section.");

        services.AddSingleton(options);

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