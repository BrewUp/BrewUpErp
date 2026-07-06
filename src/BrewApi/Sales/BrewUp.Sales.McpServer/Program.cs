using BrewUp.Sales.Infrastructure;
using BrewUp.Sales.McpServer;
using BrewUp.Sales.McpServer.Tools;
using BrewUp.Sales.ReadModel;
using BrewUp.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithTools<SalesTools>();

MongoDbSettings mongoDbSettings = builder.Configuration.GetSection("BrewUp:MongoDbSettings").Get<MongoDbSettings>()
                                  ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:MongoDbSettings'.");
builder.Services.AddMongoDb(mongoDbSettings);
builder.Services.AddScoped<IMcpSalesFacade, McpSalesFacade>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddReadModelForMcp();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();