using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Repositories;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Infrastructure;

public static class KnowledgeInfrastructureHelper
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services.AddSingleton<IKnowledgeTextExtractor, PlainTextExtractor>();
        services.AddSingleton<IKnowledgeTextExtractor, MarkdownTextExtractor>();
        services.AddSingleton<IKnowledgeTextExtractor, PdfTextExtractor>();

        if (configuration is null)
        {
            services.AddSingleton<InMemoryKnowledgeDocumentRepository>();
            services.AddSingleton<IKnowledgeDocumentRepository>(
                provider => provider.GetRequiredService<InMemoryKnowledgeDocumentRepository>());

            services.AddSingleton<InMemoryKnowledgeChunkRepository>();
            services.AddSingleton<IKnowledgeChunkRepository>(
                provider => provider.GetRequiredService<InMemoryKnowledgeChunkRepository>());
            services.AddSingleton<IKnowledgeChunkWriter>(
                provider => provider.GetRequiredService<InMemoryKnowledgeChunkRepository>());

            services.AddSingleton<InMemoryKnowledgeVectorStore>();
            services.AddSingleton<IKnowledgeVectorStore>(
                provider => provider.GetRequiredService<InMemoryKnowledgeVectorStore>());
        }
        else
        {
            var vectorStoreOptions = configuration
                .GetSection(SqlServerKnowledgeVectorStoreOptions.SectionName)
                .Get<SqlServerKnowledgeVectorStoreOptions>()
                ?? new SqlServerKnowledgeVectorStoreOptions();

            if (string.IsNullOrWhiteSpace(vectorStoreOptions.ConnectionString))
            {
                vectorStoreOptions = new SqlServerKnowledgeVectorStoreOptions
                {
                    ConnectionString =
                        configuration["BrewUp:SqlServer:ConnectionString"] ?? string.Empty,
                    Dimensions = vectorStoreOptions.Dimensions
                };
            }

            services.AddSingleton(vectorStoreOptions);
            services.AddSingleton<IKnowledgeDocumentRepository,
                SqlServerKnowledgeDocumentRepository>();
            services.AddSingleton<SqlServerKnowledgeChunkRepository>();
            services.AddSingleton<IKnowledgeChunkRepository>(
                provider => provider.GetRequiredService<SqlServerKnowledgeChunkRepository>());
            services.AddSingleton<IKnowledgeChunkWriter>(
                provider => provider.GetRequiredService<SqlServerKnowledgeChunkRepository>());
            RegisterConfiguredVectorStore(services, configuration);
        }

        var azureOptions = configuration?
            .GetSection(AzureOpenAiEmbeddingOptions.SectionName)
            .Get<AzureOpenAiEmbeddingOptions>();

        if (azureOptions is not null &&
            !string.IsNullOrWhiteSpace(azureOptions.Endpoint) &&
            !string.IsNullOrWhiteSpace(azureOptions.DeploymentName))
        {
            services.AddSingleton(azureOptions);
            services.AddSingleton<IEmbeddingGenerator, AzureOpenAiEmbeddingGenerator>();
        }
        else
        {
            services.AddSingleton<IEmbeddingGenerator, FakeEmbeddingGenerator>();
        }

        return services;
    }
    
    public static IServiceCollection AddInfrastructureForMcp(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var vectorStoreOptions = configuration!
                                     .GetSection(SqlServerKnowledgeVectorStoreOptions.SectionName)
                                     .Get<SqlServerKnowledgeVectorStoreOptions>()
                                 ?? new SqlServerKnowledgeVectorStoreOptions();
        services.AddSingleton(vectorStoreOptions);
        
        RegisterConfiguredVectorStore(services, configuration);
        
        var azureOptions = configuration
            .GetSection(AzureOpenAiEmbeddingOptions.SectionName)
            .Get<AzureOpenAiEmbeddingOptions>();
        
        services.AddSingleton<SqlServerKnowledgeChunkRepository>();
        services.AddSingleton<IKnowledgeChunkRepository>(
            provider => provider.GetRequiredService<SqlServerKnowledgeChunkRepository>());

        if (azureOptions is not null &&
            !string.IsNullOrWhiteSpace(azureOptions.Endpoint) &&
            !string.IsNullOrWhiteSpace(azureOptions.DeploymentName))
        {
            services.AddSingleton(azureOptions);
            services.AddSingleton<IEmbeddingGenerator, AzureOpenAiEmbeddingGenerator>();
        }
        else
        {
            services.AddSingleton<IEmbeddingGenerator, FakeEmbeddingGenerator>();
        }

        return services;
    }

    private static void RegisterConfiguredVectorStore(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<SqlServerKnowledgeVectorStore>();

        var vectorStore = configuration["Knowledge:VectorStore"];
        if (!string.Equals(
                vectorStore,
                "AzureAiSearch",
                StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IKnowledgeVectorStore>(
                provider => provider.GetRequiredService<SqlServerKnowledgeVectorStore>());
            return;
        }

        var options = configuration
            .GetSection(AzureAiSearchOptions.SectionName)
            .Get<AzureAiSearchOptions>()
            ?? new AzureAiSearchOptions();

        services.AddSingleton(options);
        services.AddSingleton(CreateSearchClient);
        services.AddSingleton(CreateSearchIndexClient);
        services.AddSingleton<AzureAiSearchIndexInitializer>();
        services.AddSingleton<AzureAiSearchKnowledgeVectorStore>();
        services.AddSingleton<IKnowledgeVectorStore>(
            provider => provider.GetRequiredService<AzureAiSearchKnowledgeVectorStore>());
    }

    private static SearchClient CreateSearchClient(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<AzureAiSearchOptions>();
        var endpoint = GetSearchEndpoint(options);
        var indexName = GetIndexName(options);

        if (options.UseManagedIdentity)
            return new SearchClient(endpoint, indexName, new DefaultAzureCredential());

        return new SearchClient(
            endpoint,
            indexName,
            new AzureKeyCredential(GetApiKey(options)));
    }

    private static SearchIndexClient CreateSearchIndexClient(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<AzureAiSearchOptions>();
        var endpoint = GetSearchEndpoint(options);

        if (options.UseManagedIdentity)
            return new SearchIndexClient(endpoint, new DefaultAzureCredential());

        return new SearchIndexClient(
            endpoint,
            new AzureKeyCredential(GetApiKey(options)));
    }

    private static Uri GetSearchEndpoint(AzureAiSearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new InvalidOperationException(
                $"{AzureAiSearchOptions.SectionName}:Endpoint is required.");

        return new Uri(options.Endpoint);
    }

    private static string GetIndexName(AzureAiSearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.IndexName))
            throw new InvalidOperationException(
                $"{AzureAiSearchOptions.SectionName}:IndexName is required.");

        return options.IndexName;
    }

    private static string GetApiKey(AzureAiSearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new InvalidOperationException(
                $"{AzureAiSearchOptions.SectionName}:ApiKey is required " +
                "when managed identity is disabled.");

        return options.ApiKey;
    }
}
