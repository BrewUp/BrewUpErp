using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.McpServer;
using BrewUp.Knowledge.McpServer.Tools;
using BrewUp.Knowledge.ReadModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithTools<KnowledgeTools>();

builder.Services.AddScoped<IKnowledgeFacade, KnowledgeFacade>();
builder.Services.AddInfrastructureForMcp(builder.Configuration);
builder.Services.AddKnowledgeReadModel();
    
var app = builder.Build();

app.MapMcp("/mcp");

app.Run();