using System.Net.Http.Headers;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using BrewUp.Mother.Facade.Agents;
using BrewUp.Mother.Facade.Chat;
using BrewUp.Mother.Facade.Configuration;
using BrewUp.Mother.Facade.Foundry;
using BrewUp.Mother.Facade.Mcp;
using BrewUp.Shared;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrewUp.Mother.Facade;

public static class BrewUpChatHelper
{
    public static IServiceCollection AddBrewUpChat(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLogging();
        services.AddHttpClient();
        services.AddShared();

        var a2AOptions = configuration
                             .GetSection(MotherA2AOptions.SectionName)
                             .Get<MotherA2AOptions>()
                         ?? new MotherA2AOptions();

        services.AddSingleton(a2AOptions);

        // Dedicated resilient HttpClient for MCP transports:
        //  - long enough timeout to cover multi-round tool calling
        //  - accepts both JSON and SSE responses
        //  - standard resilience pipeline (retry + circuit breaker + timeout per attempt)
        services.AddHttpClient("mcp", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(3);
                
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/event-stream"));
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
        
        var foundryOptions = configuration
                          .GetSection(FoundryLimitsOptions.SectionName)
                          .Get<FoundryLimitsOptions>()
                      ?? throw new InvalidOperationException(
                          "Missing Foundry Limit configuration section.");

        services.AddSingleton(options);
        
        // MCP clients + tool catalog are kept alive for the whole app lifetime.
        services.AddSingleton<IMcpToolsProvider, McpToolsProvider>();
        services.AddHttpClient("a2a-knowledge", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(3);

            if (!string.IsNullOrWhiteSpace(a2AOptions.KnowledgeAgentUrl))
                client.BaseAddress = new Uri(a2AOptions.KnowledgeAgentUrl.TrimEnd('/') + "/");
        });
        services.AddScoped<IKnowledgeAgentA2AClient, HttpKnowledgeAgentA2AClient>();
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
            
            var rawChatClient = azureClient
                .GetChatClient(options.DeploymentName)
                .AsIChatClient();
            
            var guardedChatClient =
                new FoundryGuardedChatClient(
                    rawChatClient,
                    foundryOptions,
                    sp.GetRequiredService<
                        ILogger<FoundryGuardedChatClient>>());

            return guardedChatClient
                .AsBuilder()
                .UseFunctionInvocation(
                    configure: functionClient =>
                    {
                        functionClient.MaximumIterationsPerRequest =
                            foundryOptions.MaximumFunctionIterations;

                        functionClient.MaximumConsecutiveErrorsPerRequest =
                            foundryOptions.MaximumConsecutiveFunctionErrors;
                    })
                .UseLogging()
                .UseOpenTelemetry(
                    sourceName: "BrewUp.Chat")
                .Build(sp);
            
            // return azureClient
            //     .GetChatClient(options.DeploymentName)
            //     .AsIChatClient()
            //     .AsBuilder()
            //     .UseFunctionInvocation()
            //     .UseLogging()
            //     .UseOpenTelemetry(sourceName: "BrewUp.Chat")
            //     .Build(sp);
        });

        services.AddScoped<BrewUpChatService>();

        return services;
    }
}
