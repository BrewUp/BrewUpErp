using BrewUp.Infrastructure;

namespace BrewUp.Rest.Module;

public class InfrastructureModule : IModule
{
    public bool IsEnabled => true;
    public int Order => 0;
    
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        using var serviceProvider = builder.Services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        builder.Services.AddInfrastructure(loggerFactory, builder.Configuration);
        
        return builder.Services;
    }

    public WebApplication Configure(WebApplication app) => app;
}