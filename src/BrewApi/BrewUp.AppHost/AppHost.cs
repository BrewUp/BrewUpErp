var builder = DistributedApplication.CreateBuilder(args);


//var serviceBus = builder.AddAzureServiceBus("messaging");

var rabbitMq = builder.AddRabbitMQ("rabbitMq");

var mongoDb = builder.AddMongoDB("mongodb")
    .WithDataVolume("brewup-mongodb-data");

var brewUpDB = mongoDb.AddDatabase("brewup-db");

var kurrent = builder.AddKurrentDB("kurrentDb")
    .WithEnvironment("KURRENTDB_ENABLE_ATOM_PUB_OVER_HTTP", "true")
    .WithDataVolume("brewup-kurrent-data");

var infra = builder.AddProject<Projects.BrewUp_Infrastructure>("infra")
    .WithReference(brewUpDB)
    .WithReference(kurrent)
    .WithReference(rabbitMq)
    .WaitFor(brewUpDB)
    .WaitFor(kurrent)
    .WaitFor(rabbitMq);

var brewUpApi = builder.AddProject<Projects.BrewUp_Rest>("api")
    .WithReference(infra)
    .WaitFor(infra);

builder.Build().Run();
