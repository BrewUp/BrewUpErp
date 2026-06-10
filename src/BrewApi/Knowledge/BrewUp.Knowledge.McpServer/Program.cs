using BrewUp.Knowledge.McpServer;
using BrewUp.Knowledge.McpServer.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithTools<KnowledgeTools>();

builder.Services.AddScoped<IKnowledgeFacade, KnowledgeFacade>();
    
var app = builder.Build();

app.MapMcp("/mcp");

app.Run();