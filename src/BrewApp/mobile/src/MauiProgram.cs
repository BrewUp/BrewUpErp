using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using BrewApp.Mobile.Services.Auth;
using BrewApp.Mobile.Services.Ui;

namespace BrewApp.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Load configuration from embedded apiSettings.example.json
        // (In production, copy to apiSettings.json and exclude from git)
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("BrewApp.Mobile.Configuration.apiSettings.example.json");
        
        builder.Configuration.AddJsonStream(stream!);

        // Register configuration strongly-typed
        builder.Services.Configure<Configuration.ApiSettings>(
            builder.Configuration.GetSection("ApiSettings"));

        // Register HttpClient factory for external APIs
        builder.Services.AddHttpClient("BrewAppAPI", (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Configuration.ApiSettings>>().Value;
            
            // Set API key header if available
            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", settings.ApiKey);
            }
            
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds > 0 ? settings.TimeoutSeconds : 30);
        });

        // Register core services
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();

        // Register API clients
        builder.Services.AddSingleton<Services.Api.ISalesApiClient, Services.Api.SalesApiClient>();
        builder.Services.AddSingleton<Services.Api.ICatalogApiClient, Services.Api.CatalogApiClient>();
        // builder.Services.AddSingleton<IOrdersApiClient, OrdersApiClient>();
        // builder.Services.AddSingleton<IWarehouseApiClient, WarehouseApiClient>();

        // Register ViewModels
        builder.Services.AddTransient<Features.Sales.SalesSummaryViewModel>();
        builder.Services.AddTransient<Features.Catalog.CatalogViewModel>();
        // builder.Services.AddTransient<OrdersListViewModel>();
        // builder.Services.AddTransient<OrderDetailViewModel>();
        // builder.Services.AddTransient<WarehouseViewModel>();

        // Register Pages
        builder.Services.AddTransient<Features.Sales.SalesPage>();
        builder.Services.AddTransient<Features.Catalog.CatalogPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
