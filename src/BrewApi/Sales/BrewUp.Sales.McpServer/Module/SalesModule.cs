using System.Net.Http.Headers;
using BrewUp.Sales.Infrastructure;
using BrewUp.Sales.McpServer.Tools;
using BrewUp.Sales.ReadModel;
using BrewUp.Shared;
using BrewUp.Shared.Agents;
using BrewUp.Shared.Configuration;

namespace BrewUp.Sales.McpServer.Module;

/// <summary>
/// Sales Module for configuring the services and endpoints in the Sales MCP
/// </summary>
public class SalesModule : IModule
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
        builder.Services
            .AddMcpServer()
            .WithHttpTransport(options =>
            {
                options.Stateless = true;
            })
            .WithTools<SalesTools>();
        
        MongoDbSettings mongoDbSettings = builder.Configuration.GetSection("BrewUp:MongoDbSettings").Get<MongoDbSettings>()
                                          ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:MongoDbSettings'.");
        builder.Services.AddMongoDb(mongoDbSettings);
        builder.Services.AddScoped<IMcpSalesFacade, McpSalesFacade>();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddReadModelForMcp();

        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapMcp("/mcp");
        app.MapGet("/health", () => Results.Ok("Sales-MCP is healthy."))
            .WithName("HealthCheck")
            .WithTags("Health");
        
        return app;
    }
}
