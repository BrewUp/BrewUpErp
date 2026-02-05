using BrewUp.MasterData.Facade;
using BrewUp.MasterData.Facade.Endpoints;

namespace BrewUp.Rest.Module;

public class MasterDataModule : IModule
{
    public bool IsEnabled => true;
    public int Order => 0;
    
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        builder.Services.AddMasterDataFacade();

        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapMasterDataEndpoints();
        
        return app;
    }
}