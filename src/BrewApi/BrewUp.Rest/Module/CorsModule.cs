namespace BrewUp.Rest.Module;

public class CorsModule : IModule
{
    public bool IsEnabled => true;
    public int Order => -1;
    
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("BrewUpCorsPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        
        return  builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.UseCors("BrewUpCorsPolicy");
        return app;
    }
}