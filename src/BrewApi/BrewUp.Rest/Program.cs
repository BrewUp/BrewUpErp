using BrewUp.Rest.Module;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterModules();

var app = builder.Build();

app.ConfigureModules();

await app.RunAsync();
