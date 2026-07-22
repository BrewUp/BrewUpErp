namespace Aspire.Hosting;

public static class BrewUpContainerExtensions
{
    // Registers the BrewUp MCP servers and the Knowledge Agent as containers on the AppHost.
    public static IDistributedApplicationBuilder AddBrewUpMcpContainers(
        this IDistributedApplicationBuilder builder,
        BrewUpParameters parameters)
    {
        // ---------------------------------------------------------------------
        // MCP servers.
        // Image names must match the images already available in Docker or in
        // your container registry.
        // ---------------------------------------------------------------------

        var knowledgeMcp = builder.AddBrewUpMcpServer(
            "knowledge-mcp",
            "brewup.knowledge.mcpserver",
            8081,
            parameters);

        var masterDataMcp = builder.AddBrewUpMcpServer(
            "masterdata-mcp",
            "brewup.masterdata.mcpserver",
            8082,
            parameters);

        var warehouseMcp = builder.AddBrewUpMcpServer(
            "warehouse-mcp",
            "brewup.warehouse.mcpserver",
            8083,
            parameters);

        var salesMcp = builder.AddBrewUpMcpServer(
            "sales-mcp",
            "brewup.sales.mcpserver",
            8084,
            parameters);

        // ---------------------------------------------------------------------
        // Knowledge Agent.
        // The MCP endpoint is resolved by Aspire; localhost must not be used for
        // container-to-container communication.
        // ---------------------------------------------------------------------

        var knowledgeAgent = builder
            .AddContainer(
                "knowledge-agent",
                "brewup.knowledge.agent",
                "latest")
            .WithBrewUpInfrastructure(parameters)
            .WithHttpEndpoint(
                name: "http",
                port: 8080,
                targetPort: 8080)
            .WithReference(knowledgeMcp.GetEndpoint("http"))
            .WaitFor(knowledgeMcp)
            .WithEnvironment(
                "KnowledgeAgent__Mcp__ServerName",
                "knowledge")
            .WithEnvironment(
                "KnowledgeAgent__Mcp__Endpoint",
                ReferenceExpression.Create(
                    $"{knowledgeMcp.GetEndpoint("http")}/mcp"))
            .WithEnvironment(
                "KnowledgeAgent__Mcp__DefaultTopK",
                "5");
        
        var restApi = builder
            .AddProject<Projects.BrewUp_Rest>("brewup-rest")
            .WithReference(knowledgeMcp.GetEndpoint("http"))
            .WithReference(masterDataMcp.GetEndpoint("http"))
            .WithReference(warehouseMcp.GetEndpoint("http"))
            .WithReference(salesMcp.GetEndpoint("http"))
            .WaitFor(knowledgeMcp)
            .WaitFor(masterDataMcp)
            .WaitFor(warehouseMcp)
            .WaitFor(salesMcp);

        return builder;
    }

    // Adds a single BrewUp MCP server container with the shared infrastructure
    // configuration and the standard HTTP endpoint.
    private static IResourceBuilder<ContainerResource> AddBrewUpMcpServer(
        this IDistributedApplicationBuilder builder,
        string name,
        string image,
        int port,
        BrewUpParameters parameters)
    {
        return builder
            .AddContainer(name, image, "latest")
            .WithBrewUpInfrastructure(parameters)
            .WithHttpEndpoint(
                name: "http",
                port: port,
                targetPort: 8080)
            
            // Inietta nel container l'endpoint OTLP della dashboard Aspire.
            .WithOtlpExporter()
            
            .WithEnvironment(
                "BrewUp__MongoDbSettings__ConnectionString",
                parameters.MongoConnectionString)

            .WithEnvironment(
                "BrewUp__SqlServer__ConnectionString",
                parameters.SqlServerConnectionString)

            .WithEnvironment(
                "BrewUp__AzureServiceBus__ConnectionString",
                parameters.ServiceBusConnectionString);
    }
}
