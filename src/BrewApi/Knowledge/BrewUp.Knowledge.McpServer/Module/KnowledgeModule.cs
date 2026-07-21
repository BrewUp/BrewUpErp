using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.McpServer.Tools;
using BrewUp.Knowledge.ReadModel;

namespace BrewUp.Knowledge.McpServer.Module;

/// <summary>
/// Knowledge Module for configuring the services and endpoints in the Knowledge MCP
/// </summary>
public class KnowledgeModule : IModule
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
        builder.AddServiceDefaults();
        
        builder.Services
            .AddMcpServer()
            .WithHttpTransport(options =>
            {
                options.Stateless = true;
            })
            .WithTools<KnowledgeTools>();

        builder.Services.AddScoped<IKnowledgeFacade, KnowledgeFacade>();
        builder.Services.AddInfrastructureForMcp(builder.Configuration);
        builder.Services.AddKnowledgeReadModel();

        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapMcp("/mcp");
        app.MapGet("/health", () => Results.Ok("Knowledge-MCP is healthy."))
            .WithName("HealthCheck")
            .WithTags("Health");

        // Espone /health e /alive e completa la configurazione Aspire.
        app.MapDefaultEndpoints();

        return app;
    }
}
