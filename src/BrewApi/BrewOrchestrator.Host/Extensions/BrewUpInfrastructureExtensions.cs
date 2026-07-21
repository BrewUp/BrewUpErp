namespace Aspire.Hosting;

public static class BrewUpInfrastructureExtensions
{
    // Applies the shared BrewUp infrastructure environment configuration to a container.
    // This preserves the exact variable names already used by docker-compose.
    public static IResourceBuilder<ContainerResource> WithBrewUpInfrastructure(
        this IResourceBuilder<ContainerResource> resource,
        BrewUpParameters parameters)
    {
        return resource
            // MongoDB Atlas
            .WithEnvironment(
                "BrewUp__MongoDbSettings__ConnectionString",
                parameters.MongoConnectionString)
            .WithEnvironment(
                "BrewUp__MongoDbSettings__DatabaseName",
                "BrewUp")

            // EventStore
            .WithEnvironment(
                "BrewUp__EventStore__ConnectionString",
                parameters.EventStoreConnectionString)

            // RabbitMQ
            .WithEnvironment(
                "BrewUp__RabbitMQ__Host",
                "localhost")
            .WithEnvironment(
                "BrewUp__RabbitMQ__ExchangeCommandName",
                parameters.RabbitMqExchangeCommandName)
            .WithEnvironment(
                "BrewUp__RabbitMQ__ExchangeEventName",
                parameters.RabbitMqExchangeEventName)
            .WithEnvironment(
                "BrewUp__RabbitMQ__Username",
                parameters.RabbitMqUsername)
            .WithEnvironment(
                "BrewUp__RabbitMQ__Password",
                parameters.RabbitMqPassword)
            .WithEnvironment(
                "BrewUp__RabbitMQ__ClientId",
                "BrewUp")
            .WithEnvironment(
                "BrewUp__RabbitMQ__UseRMQ",
                "false")

            // Azure Service Bus
            .WithEnvironment(
                "BrewUp__AzureServiceBus__ConnectionString",
                parameters.ServiceBusConnectionString)
            .WithEnvironment(
                "BrewUp__AzureServiceBus__TopicName",
                parameters.ServiceBusTopicName)
            .WithEnvironment(
                "BrewUp__AzureServiceBus__ClientId",
                parameters.ServiceBusClientId)
            .WithEnvironment(
                "BrewUp__AzureServiceBus__MaxConcurrentCalls",
                "1")
            .WithEnvironment(
                "BrewUp__AzureServiceBus__UseAzureServiceBus",
                "true")

            // SQL Server
            .WithEnvironment(
                "BrewUp__SqlServer__ConnectionString",
                parameters.SqlServerConnectionString)
            .WithEnvironment(
                "BrewUp__SqlServer__SnapshotSize",
                "1000")
            .WithEnvironment(
                "BrewUp__SqlServer__Dimensions",
                "1536")

            // Azure OpenAI - Chat
            .WithEnvironment(
                "BrewUp__AzureOpenAI__Endpoint",
                parameters.AzureOpenAiEndpoint)
            .WithEnvironment(
                "BrewUp__AzureOpenAI__DeploymentName",
                "mistral-small-2503")
            .WithEnvironment(
                "BrewUp__AzureOpenAI__ApiKey",
                parameters.AzureOpenAiApiKey)
            .WithEnvironment(
                "BrewUp__AzureOpenAI__TenantId",
                parameters.AzureOpenAiTenantId)
            .WithEnvironment(
                "BrewUp__AzureOpenAI__UseManagedIdentity",
                "false")

            // Azure OpenAI - Embeddings
            .WithEnvironment(
                "BrewUp__Embeddings__Endpoint",
                parameters.EmbeddingsEndpoint)
            .WithEnvironment(
                "BrewUp__Embeddings__DeploymentName",
                "text-embedding-3-small")
            .WithEnvironment(
                "BrewUp__Embeddings__Dimensions",
                "1536")
            .WithEnvironment(
                "BrewUp__Embeddings__ApiKey",
                parameters.EmbeddingsApiKey)
            .WithEnvironment(
                "BrewUp__Embeddings__TenantId",
                parameters.EmbeddingsTenantId)
            .WithEnvironment(
                "BrewUp__Embeddings__UseManagedIdentity",
                "false")

            // Knowledge
            .WithEnvironment(
                "Knowledge__VectorStore",
                "SqlServer")

            // ASP.NET Core
            .WithEnvironment(
                "ASPNETCORE_HTTP_PORTS",
                "8080");
    }
}
