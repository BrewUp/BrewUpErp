using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BrewSpa;
using BrewSpa.MasterData.Application.Extensions;
using BrewSpa.Shared.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

builder.Services.AddSharedComponents();

builder.Services.AddMasterDataServices(builder.Configuration);

await builder.Build().RunAsync();