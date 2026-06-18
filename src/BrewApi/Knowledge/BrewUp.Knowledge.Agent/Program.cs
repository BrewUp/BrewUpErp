using BrewUp.Knowledge.Agent.Module;

var builder = WebApplication.CreateBuilder(args);

// Explicit composition-root pattern for better control and visibility of module registration and configuration
builder.RegisterModules([
    new OpenApiModule(),
    new AgentModule()
]);

var app = builder.Build();

app.ConfigureModules();

await app.RunAsync();