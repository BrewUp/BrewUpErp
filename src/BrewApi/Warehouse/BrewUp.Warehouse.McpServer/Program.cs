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
    
MongoDbSettings mongoDbSettings = builder.Configuration.GetSection("BrewUp:MongoDbSettings").Get<MongoDbSettings>()
                                  ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:MongoDbSettings'.");
builder.Services.AddMongoDb(mongoDbSettings);
builder.Services.AddScoped<IMcpWarehouseFacade, McpWarehouseFacade>();
builder.Services.AddInfrastructure();
builder.Services.AddReadModelForMcp();
    
var app = builder.Build();

app.MapMcp("/mcp");

app.Run();