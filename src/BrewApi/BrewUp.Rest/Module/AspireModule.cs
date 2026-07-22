namespace BrewUp.Rest.Module;

/// <summary>
/// AspireModule for Aspire configuration.
/// </summary>
public static class AspireModule
{
    /// <summary>
    /// Registers the module's services and dependencies in the application's service collection.
    /// This method is called during the application startup process to configure the module's services.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection Register(WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        
        return builder.Services;
    }

    /// <summary>
    /// Configures the module's middleware and request pipeline in the application.
    /// This method is called during the application startup process to set up the module's middleware and request handling logic.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication Configure(WebApplication app)
    {
        app.MapDefaultEndpoints();
        
        return app;
    }
}