using BrewUp.Sales.Facade;
using BrewUp.Sales.Facade.Endpoints;

namespace BrewUp.Rest.Module;

public class SalesModule : IModule
{
    public bool IsEnabled => true;
    public int Order => 10;
    
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        using var serviceProvider = builder.Services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        builder.Services.AddSalesFacade(loggerFactory, builder.Configuration);
        
        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapSalesEndpoints();
        
        return app;
    }
}