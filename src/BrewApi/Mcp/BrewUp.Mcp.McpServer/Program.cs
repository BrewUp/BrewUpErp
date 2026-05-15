using BrewUp.MasterData.ReadModel;
using BrewUp.Mcp.Facade;
using BrewUp.Mcp.McpServer;
using BrewUp.Mcp.McpServer.Tools;
using BrewUp.Sales.Infrastructure;
using BrewUp.Sales.ReadModel;
using BrewUp.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithTools<BrewUpMcpTools>();

MongoDbSettings mongoDbSettings = new();
builder.Configuration.GetSection("BrewUp:MongoDbSettings").Bind(mongoDbSettings);
builder.Services.AddMongoDb(mongoDbSettings);
builder.Services.AddMasterDataReadModel();
builder.Services.AddSalesInfrastructure(builder.Configuration);
builder.Services.AddSalesReadModelForMcp();
builder.Services.AddBrewUpAi(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "BrewUp MCP Server is running");
app.MapMcp("/mcp");

app.Run();
