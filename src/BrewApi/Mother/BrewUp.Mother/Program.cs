using BrewUp.Mother;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddMother();

var host = builder.Build();
host.Run();
