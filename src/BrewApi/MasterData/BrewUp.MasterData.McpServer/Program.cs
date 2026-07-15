using BrewUp.MasterData.McpServer.Module;

var builder = WebApplication.CreateBuilder(args);

// Explicit composition-root pattern for better control and visibility of module registration and configuration
builder.RegisterModules([
    new MasterDataModule()
]);

var app = builder.Build();
app.ConfigureModules();

app.Run();