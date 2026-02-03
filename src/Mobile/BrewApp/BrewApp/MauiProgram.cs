using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Toolkit.Hosting;
using BrewApp.Services.Sales;
using BrewApp.PageModels.Sales;
using BrewApp.Pages.Sales;
using BrewApp.Configuration;

namespace BrewApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
    				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
    				{
    					handler.PlatformView.SingleSelectionFollowsFocus = false;
    				});

    				Microsoft.Maui.Handlers.ContentViewHandler.Mapper.AppendToMapping(nameof(Pages.Sales.SalesOrderListPage), (handler, view) =>
    				{
    					if (view is Pages.Sales.SalesOrderListPage && handler.PlatformView is Microsoft.Maui.Platform.ContentPanel contentPanel)
    					{
    						contentPanel.IsTabStop = true;
    					}
    				});
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            // Register HttpClient for API calls - using manual registration instead of AddHttpClient
            builder.Services.AddSingleton<ISalesOrderService>(sp =>
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(AppSettings.ApiBaseUrl)
                };
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                return new SalesOrderService(httpClient);
            });

            // Register Services
            builder.Services.AddSingleton<ModalErrorHandler>();

            // Register ViewModels
            builder.Services.AddSingleton<SalesOrderListPageModel>();
            builder.Services.AddTransient<SalesOrderDetailPageModel>();
            builder.Services.AddTransient<CreateSalesOrderPageModel>();

            // Register Pages
            builder.Services.AddSingleton<SalesOrderListPage>();
            builder.Services.AddTransientWithShellRoute<SalesOrderDetailPage, SalesOrderDetailPageModel>("orderdetail");
            builder.Services.AddTransientWithShellRoute<CreateSalesOrderPage, CreateSalesOrderPageModel>("createorder");

            return builder.Build();
        }
    }
}
