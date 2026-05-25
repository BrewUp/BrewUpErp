// using BrewUp.Mother;
//
// var builder = Host.CreateApplicationBuilder(args);
// builder.Services.AddHostedService<Worker>();
//
// builder.Services.AddMother(builder.Configuration);
//
// var host = builder.Build();
// host.Run();

using BrewUp.Mother;
using BrewUp.Mother.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddMother(builder.Configuration);

var app = builder.Build();

app.MapHub<MotherHub>("/hubs/mother");

app.Run();
