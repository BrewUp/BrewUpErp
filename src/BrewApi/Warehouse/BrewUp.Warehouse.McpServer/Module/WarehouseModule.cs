using BrewUp.Shared.Configuration;
using BrewUp.Warehouse.Infrastructure;
using BrewUp.Warehouse.McpServer.Tools;
using BrewUp.Warehouse.ReadModel;

namespace BrewUp.Warehouse.McpServer.Module;

/// <summary>
/// Warehouse Module for configuring the services and endpoints in the Warehouse MCP
/// </summary>
public class WarehouseModule : IModule
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
            .WithTools<WarehouseTools>();

        MongoDbSettings mongoDbSettings = builder.Configuration.GetSection("BrewUp:MongoDbSettings").Get<MongoDbSettings>()
                                          ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:MongoDbSettings'.");
        builder.Services.AddMongoDb(mongoDbSettings);
        builder.Services.AddScoped<IMcpWarehouseFacade, McpWarehouseFacade>();
        builder.Services.AddInfrastructure();
        builder.Services.AddReadModelForMcp();

        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapMcp("/mcp");
        app.MapGet("/health", () => Results.Ok("Warehouse-MCP is healthy."))
            .WithName("HealthCheck")
            .WithTags("Health");
        
        // Espone /health e /alive e completa la configurazione Aspire.
        app.MapDefaultEndpoints();

        return app;
    }
}
