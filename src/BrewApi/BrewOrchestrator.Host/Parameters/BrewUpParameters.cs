namespace Aspire.Hosting;

// Strongly-typed bundle of the AppHost parameters.
// Sensitive values are stored in the AppHost user-secrets under "Parameters:<parameter-name>".
public sealed record BrewUpParameters(
    IResourceBuilder<ParameterResource> MongoConnectionString,
    IResourceBuilder<ParameterResource> EventStoreConnectionString,
    IResourceBuilder<ParameterResource> RabbitMqExchangeCommandName,
    IResourceBuilder<ParameterResource> RabbitMqExchangeEventName,
    IResourceBuilder<ParameterResource> RabbitMqUsername,
    IResourceBuilder<ParameterResource> RabbitMqPassword,
    IResourceBuilder<ParameterResource> ServiceBusConnectionString,
    IResourceBuilder<ParameterResource> ServiceBusTopicName,
    IResourceBuilder<ParameterResource> ServiceBusClientId,
    IResourceBuilder<ParameterResource> SqlServerConnectionString,
    IResourceBuilder<ParameterResource> AzureOpenAiEndpoint,
    IResourceBuilder<ParameterResource> AzureOpenAiApiKey,
    IResourceBuilder<ParameterResource> AzureOpenAiTenantId,
    IResourceBuilder<ParameterResource> EmbeddingsEndpoint,
    IResourceBuilder<ParameterResource> EmbeddingsApiKey,
    IResourceBuilder<ParameterResource> EmbeddingsTenantId);

public static class BrewUpParameterExtensions
{
    // Registers every AppHost parameter and returns them as a strongly-typed bundle.
    public static BrewUpParameters AddBrewUpParameters(this IDistributedApplicationBuilder builder)
    {
        return new BrewUpParameters(
            MongoConnectionString: builder.AddParameter(
                "mongo-connection-string",
                secret: true),
            EventStoreConnectionString: builder.AddParameter(
                "eventstore-connection-string",
                secret: true),
            RabbitMqExchangeCommandName: builder.AddParameter(
                "rabbitmq-exchange-command-name"),
            RabbitMqExchangeEventName: builder.AddParameter(
                "rabbitmq-exchange-event-name"),
            RabbitMqUsername: builder.AddParameter(
                "rabbitmq-username"),
            RabbitMqPassword: builder.AddParameter(
                "rabbitmq-password",
                secret: true),
            ServiceBusConnectionString: builder.AddParameter(
                "servicebus-connection-string",
                secret: true),
            ServiceBusTopicName: builder.AddParameter(
                "servicebus-topic-name"),
            ServiceBusClientId: builder.AddParameter(
                "servicebus-client-id"),
            SqlServerConnectionString: builder.AddParameter(
                "sqlserver-connection-string",
                secret: true),
            AzureOpenAiEndpoint: builder.AddParameter(
                "azure-openai-endpoint"),
            AzureOpenAiApiKey: builder.AddParameter(
                "azure-openai-api-key",
                secret: true),
            AzureOpenAiTenantId: builder.AddParameter(
                "azure-openai-tenant-id"),
            EmbeddingsEndpoint: builder.AddParameter(
                "embeddings-endpoint"),
            EmbeddingsApiKey: builder.AddParameter(
                "embeddings-api-key",
                secret: true),
            EmbeddingsTenantId: builder.AddParameter(
                "embeddings-tenant-id"));
    }
}
