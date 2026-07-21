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

        // ---------------------------------------------------------------------
        // Optional: Mother.
        // Uncomment this block after replacing the image name with the real one.
        // ---------------------------------------------------------------------

        /*
        var mother = builder
            .AddContainer(
                "mother",
                "brewup.mother",
                "latest")
            .WithBrewUpInfrastructure(parameters)
            .WithHttpEndpoint(
                name: "http",
                targetPort: 8080)
            .WithReference(knowledgeAgent.GetEndpoint("http"))
            .WithReference(salesMcp.GetEndpoint("http"))
            .WithReference(warehouseMcp.GetEndpoint("http"))
            .WithReference(masterDataMcp.GetEndpoint("http"))
            .WaitFor(knowledgeAgent)
            .WaitFor(salesMcp)
            .WaitFor(warehouseMcp)
            .WaitFor(masterDataMcp)
            .WithEnvironment(
                "BrewUp__Mother__A2A__Enabled",
                "true")
            .WithEnvironment(
                "BrewUp__Mother__A2A__KnowledgeAgentUrl",
                knowledgeAgent.GetEndpoint("http"))
            .WithEnvironment(
                "BrewUp__McpServers__MotherUrl",
                ReferenceExpression.Create(
                    $"{mother.GetEndpoint("http")}/mcp"))
            .WithEnvironment(
                "BrewUp__McpServers__SalesUrl",
                ReferenceExpression.Create(
                    $"{salesMcp.GetEndpoint("http")}/mcp"))
            .WithEnvironment(
                "BrewUp__McpServers__WarehouseUrl",
                ReferenceExpression.Create(
                    $"{warehouseMcp.GetEndpoint("http")}/mcp"))
            .WithEnvironment(
                "BrewUp__McpServers__MasterDataUrl",
                ReferenceExpression.Create(
                    $"{masterDataMcp.GetEndpoint("http")}/mcp"))
            .WithEnvironment(
                "BrewUp__McpServers__KnowledgeUrl",
                ReferenceExpression.Create(
                    $"{knowledgeMcp.GetEndpoint("http")}/mcp"));
        */

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
