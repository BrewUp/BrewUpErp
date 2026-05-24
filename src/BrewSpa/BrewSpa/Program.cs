using BrewSpa;
using BrewSpa.Chat.Application.Extensions;
using BrewSpa.Dashboards.ApplicationServices.Extensions;
using BrewSpa.MasterData.Application.Extensions;
using BrewSpa.Sales.Application.Extensions;
using BrewSpa.Shared.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.AddSharedComponents();

builder.Services.AddMasterDataServices(builder.Configuration);
builder.Services.AddDashboardsServices(builder.Configuration);
builder.Services.AddSalesServices(builder.Configuration);
builder.Services.AddChatServices(builder.Configuration);

await builder.Build().RunAsync();