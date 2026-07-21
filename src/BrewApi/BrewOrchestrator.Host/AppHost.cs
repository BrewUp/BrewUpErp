var builder = DistributedApplication.CreateBuilder(args);

// Register the AppHost parameters. Secrets live in the AppHost user-secrets
// under "Parameters:<parameter-name>".
var parameters = builder.AddBrewUpParameters();

// Register the BrewUp MCP servers and the Knowledge Agent.
builder.AddBrewUpMcpContainers(parameters);

builder.Build().Run();
