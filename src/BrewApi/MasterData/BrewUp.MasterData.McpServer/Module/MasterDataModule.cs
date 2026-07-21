using BrewUp.MasterData.Infrastructure;
using BrewUp.MasterData.McpServer.Tools;
using BrewUp.MasterData.ReadModel;
using BrewUp.Shared.Configuration;

namespace BrewUp.MasterData.McpServer.Module;

/// <summary>
/// MasterData Module for configuring the services and endpoints in the MasterData MCP
/// </summary>
public class MasterDataModule : IModule
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
            .WithTools<MasterDataTools>();

        MongoDbSettings mongoDbSettings = builder.Configuration.GetSection("BrewUp:MongoDbSettings").Get<MongoDbSettings>()
                                          ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:MongoDbSettings'.");
        builder.Services.AddMongoDb(mongoDbSettings);
        builder.Services.AddScoped<IMcpMasterDataFacade, McpMasterDataFacade>();
        builder.Services.AddMasterDataInfrastructure();
        builder.Services.AddMasterDataReadModel();

        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapMcp("/mcp");
        app.MapGet("/health", () => Results.Ok("MasterData-MCP is healthy."))
            .WithName("HealthCheck")
            .WithTags("Health");

        // Espone /health e /alive e completa la configurazione Aspire.
        app.MapDefaultEndpoints();
        
        return app;
    }
}
