using BrewUp.MasterData.Infrastructure;
using BrewUp.MasterData.McpServer;
using BrewUp.MasterData.McpServer.Tools;
using BrewUp.MasterData.ReadModel;
using BrewUp.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithTools<MasterDataTools>();
    
MongoDbSettings mongoDbSettings = builder.Configuration.GetSection("BrewUp:MongoDbSettings").Get<MongoDbSettings>()
                                  ?? throw new InvalidOperationException("Missing configuration section 'BrewUp:MongoDbSettings'.");
builder.Services.AddMongoDb(mongoDbSettings);
builder.Services.AddScoped<IMcpMasterDataFacade, McpMasterDataFacade>();
builder.Services.AddMasterDataInfrastructure();
builder.Services.AddMasterDataReadModel();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();