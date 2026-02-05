using BrewUp.Sales.Facade;
using BrewUp.Sales.Facade.Endpoints;

namespace BrewUp.Rest.Module;

public class SalesModule : IModule
{
    public bool IsEnabled => true;
    public int Order => 0;
    
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        builder.Services.AddSalesFacade(builder.Configuration);
        
        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapSalesEndpoints();
        
        return app;
    }
}