using BrewUp.Mother;
using BrewUp.Mother.Mcp;
using BrewUp.Mother.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithTools<MotherTools>();

// Explicit configuration loading from appsettings.json and environment-specific overrides
var configuration = builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var mcpServerOptions = configuration
                           .GetSection(McpServerOptions.SectionName)
                           .Get<McpServerOptions>()
                       ?? throw new InvalidOperationException(
                           "Missing BrewUp:McpServers configuration section.");

builder.Services.AddHttpClient();
builder.Services.AddSingleton(mcpServerOptions);
builder.Services.AddSingleton<IMcpToolsProvider, McpToolsProvider>();
builder.Services.AddScoped<IMcpMotherFacade, McpMotherFacade>();
    
var app = builder.Build();

app.MapMcp("/mcp");

app.Run();