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

        if (configuration is null)
        {
            services.AddSingleton<InMemoryKnowledgeDocumentRepository>();
            services.AddSingleton<IKnowledgeDocumentRepository>(
                provider => provider.GetRequiredService<InMemoryKnowledgeDocumentRepository>());

            services.AddSingleton<SqlServerKnowledgeChunkRepository>();
            services.AddSingleton<SqlServerKnowledgeChunkRepository>(
                provider => provider.GetRequiredService<SqlServerKnowledgeChunkRepository>());
            services.AddSingleton<IKnowledgeChunkWriter>(
                provider => provider.GetRequiredService<SqlServerKnowledgeChunkRepository>());

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
            services.AddSingleton<IKnowledgeVectorStore, SqlServerKnowledgeVectorStore>();
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
}
