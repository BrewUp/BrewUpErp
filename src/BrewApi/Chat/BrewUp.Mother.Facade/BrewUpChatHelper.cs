using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using BrewUp.Mother.Facade.Agents;
using BrewUp.Mother.Facade.Chat;
using BrewUp.Mother.Facade.Mcp;
using BrewUp.Shared;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Mother.Facade;

public static class BrewUpChatHelper
{
    public static IServiceCollection AddBrewUpChat(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLogging();
        services.AddHttpClient();
        services.AddShared();

        // Dedicated resilient HttpClient for MCP transports:
        //  - long enough timeout to cover multi-round tool calling
        //  - standard resilience pipeline (retry + circuit breaker + timeout per attempt)
        services.AddHttpClient("mcp", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(3);
            })
            .AddStandardResilienceHandler(o =>
            {
                o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
                o.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(3);
            });

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
        
        // MCP clients + tool catalog are kept alive for the whole app lifetime.
        services.AddSingleton<IMcpToolsProvider, McpToolsProvider>();
        services.AddScoped<MasterDataAgent>();
        services.AddScoped<SalesAgent>();
        services.AddScoped<WarehouseAgent>();
        services.AddScoped<IAgent>(sp => sp.GetRequiredService<MasterDataAgent>());
        services.AddScoped<IAgent>(sp => sp.GetRequiredService<SalesAgent>());
        services.AddScoped<IAgent>(sp => sp.GetRequiredService<WarehouseAgent>());
        services.AddScoped<MotherCoordinator>();
        
        services.AddSingleton<IChatClient>(sp =>
        {
            // Extend the per-request network timeout to survive multi-round tool calls.
            var azureOptions = new AzureOpenAIClientOptions
            {
                NetworkTimeout = TimeSpan.FromMinutes(3)
            };

            AzureOpenAIClient azureClient;
            if (options.UseManagedIdentity)
            {
                var credentialOptions = new DefaultAzureCredentialOptions
                {
                    Diagnostics =
                    {
                        IsLoggingEnabled = true,
                        IsAccountIdentifierLoggingEnabled = true,
                        IsTelemetryEnabled = false,
                    },
                    // Pin the tenant when set, to avoid cross-tenant 401s in dev.
                    TenantId = Environment.GetEnvironmentVariable(options.TenantId),
                };

                TokenCredential credential = new DefaultAzureCredential(credentialOptions);
                azureClient = new AzureOpenAIClient(new Uri(options.Endpoint), credential, azureOptions);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(options.ApiKey))
                    throw new InvalidOperationException(
                        "AzureOpenAI:ApiKey is required when UseManagedIdentity is false.");

                azureClient = new AzureOpenAIClient(
                    new Uri(options.Endpoint),
                    new AzureKeyCredential(options.ApiKey),
                    azureOptions);
            }

            return azureClient
                .GetChatClient(options.DeploymentName)
                .AsIChatClient()
                .AsBuilder()
                .UseFunctionInvocation()
                .UseLogging()
                .UseOpenTelemetry(sourceName: "BrewUp.Chat")
                .Build(sp);
        });

        services.AddScoped<BrewUpChatService>();

        return services;
    }
}
