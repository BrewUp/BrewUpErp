var builder = DistributedApplication.CreateBuilder(args);

// // var kurrent = builder.AddKurrentDB("kurrentDb")
// //     .WithEnvironment("KURRENTDB_ENABLE_ATOM_PUB_OVER_HTTP", "true")
// //     .WithDataVolume("brewup-kurrent-data");
// var brewUpApi = builder.AddProject<Projects.BrewUp_Rest>("brewUpApi")
//     // .WithReference(kurrent)
//     // .WaitFor(kurrent)
//     // .WithEnvironment("BrewUp__EventStore__ConnectionString", kurrent.Resource.ConnectionStringExpression);

builder.Build().Run();
