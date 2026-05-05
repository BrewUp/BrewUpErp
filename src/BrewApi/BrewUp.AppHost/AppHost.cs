var builder = DistributedApplication.CreateBuilder(args);

var brewUpApi = builder.AddProject<Projects.BrewUp_Rest>("brewUpApi");

builder.Build().Run();
