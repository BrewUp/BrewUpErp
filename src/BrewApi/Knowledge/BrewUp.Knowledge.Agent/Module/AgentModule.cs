using System.Net.Http.Headers;
using BrewUp.Shared;
using BrewUp.Shared.Agents;

namespace BrewUp.Knowledge.Agent.Module;

/// <summary>
/// Agent Module for configuring the services and endpoints in the Agent
/// </summary>
public class AgentModule : IModule
{
    /// <summary>
    /// Indicates whether the module is enabled and should be registered in the application.
    /// </summary>
    public bool IsEnabled => true;
    /// <summary>
    /// Set the order in which the module should be registered in the application.
    /// Modules with lower order values will be registered before those with higher values.
    /// </summary>
    public int Order => 0;
    
    /// <summary>
    /// Registers the module's services and dependencies in the application's service collection.
    /// This method is called during the application startup process to configure the module's services.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        builder.Services.AddLogging();
        builder.Services.AddHttpClient();
        builder.Services.AddShared();
        
        builder.Services.AddHttpClient("mcp", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(3);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        });

        builder.Services.AddScoped<BrewUpKnowledgeAgentCardProvider>();
        builder.Services.AddScoped<IAgentCardProvider>(sp => sp.GetRequiredService<BrewUpKnowledgeAgentCardProvider>());
        builder.Services.AddScoped<KnowledgeAgentExecutor>();

        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapGet("/.well-known/agent-card.json", (
            BrewUpKnowledgeAgentCardProvider provider,
            ILoggerFactory loggerFactory) =>
        {
            var card = provider.GetAgentCard();
            loggerFactory
                .CreateLogger("BrewUp.Knowledge.Agent.A2A")
                .LogInformation("KnowledgeAgent exposed Agent Card");

            return Results.Ok(card);
        });

        app.MapGet("/a2a/agent-card", (BrewUpKnowledgeAgentCardProvider provider) => Results.Ok(provider.GetAgentCard()));

        app.MapPost("/a2a/tasks", async (
            A2ATaskRequest request,
            KnowledgeAgentExecutor executor,
            CancellationToken cancellationToken) =>
        {
            var response = await executor.ExecuteAsync(request, cancellationToken);
            return Results.Ok(response);
        });
        
        return app;
    }
}