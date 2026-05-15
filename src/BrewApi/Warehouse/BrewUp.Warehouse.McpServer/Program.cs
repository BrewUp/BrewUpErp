using BrewUp.Shared.Configuration;
using BrewUp.Warehouse.Infrastructure;
using BrewUp.Warehouse.McpServer;
using BrewUp.Warehouse.McpServer.Tools;
using BrewUp.Warehouse.ReadModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithTools<WarehouseTools>();
    
MongoDbSettings mongoDbSettings = new();
builder.Configuration.GetSection("BrewUp:MongoDbSettings").Bind(mongoDbSettings);
builder.Services.AddMongoDb(mongoDbSettings);
builder.Services.AddScoped<IMcpWarehouseFacade, McpWarehouseFacade>();
builder.Services.AddInfrastructure();
builder.Services.AddReadModelForMcp();
    
var app = builder.Build();

app.MapMcp("/mcp");

app.Run();