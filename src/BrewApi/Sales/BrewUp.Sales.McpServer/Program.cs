using BrewUp.Sales.Infrastructure;
using BrewUp.Sales.McpServer;
using BrewUp.Sales.McpServer.Tools;
using BrewUp.Sales.ReadModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithTools<SalesTools>();

builder.Services.AddScoped<IMcpSalesFacade, McpSalesFacade>();
builder.Services.AddSalesInfrastructure(builder.Configuration);
builder.Services.AddSalesReadModelForMcp();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();