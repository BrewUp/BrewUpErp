using BrewUp.Knowledge.Infrastructure.Ingestion;
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
