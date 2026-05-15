using BrewUp.AI.Facade;
using BrewUp.AI.McpServer;
using BrewUp.AI.McpServer.Tools;
using BrewUp.MasterData.ReadModel;
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
builder.Services.AddSalesReadModelForChat();
builder.Services.AddBrewUpAi(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "BrewUp MCP Server is running");
app.MapMcp("/mcp");

app.Run();
