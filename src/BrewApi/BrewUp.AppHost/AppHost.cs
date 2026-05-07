var builder = DistributedApplication.CreateBuilder(args);


//var serviceBus = builder.AddServiceBus

var rabbitmq = builder.AddRabbitMQ("messaging");


var mongoDb = builder.AddMongoDB("mongoDb")
    .WithDataVolume("brewup-mongodb-data");

var kurrent = builder.AddKurrentDB("kurrentDb")
    .WithEnvironment("KURRENTDB_ENABLE_ATOM_PUB_OVER_HTTP", "true")
    .WithDataVolume("brewup-kurrent-data");

var infra = builder.AddProject<Projects.BrewUp_Infrastructure>("infra")
    .WithReference(mongoDb)
    .WithReference(kurrent)
    .WaitFor(mongoDb)
    .WaitFor(kurrent);

var brewUpApi = builder.AddProject<Projects.BrewUp_Rest>("brewUpApi")
    .WithReference(infra)
    .WaitFor(infra);

builder.Build().Run();
