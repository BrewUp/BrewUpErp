var builder = DistributedApplication.CreateBuilder(args);

var rabbitMq = builder.AddRabbitMQ("rabbitMq", port: 5672)
    .WithManagementPlugin();

var mongoDb = builder.AddMongoDB("mongodb")
    .WithDataVolume("brewup-mongodb-data");

var brewUpDB = mongoDb.AddDatabase("brewup-db");

var kurrent = builder.AddKurrentDB("kurrentDb")
    .WithEnvironment("KURRENTDB_ENABLE_ATOM_PUB_OVER_HTTP", "true")
    .WithDataVolume("brewup-kurrent-data");

var brewUpApi = builder.AddProject<Projects.BrewUp_Rest>("api")
    .WithReference(brewUpDB)
    .WithReference(kurrent)
    .WithReference(rabbitMq)
    .WaitFor(brewUpDB)
    .WaitFor(kurrent)
    .WaitFor(rabbitMq);

builder.Build().Run();
