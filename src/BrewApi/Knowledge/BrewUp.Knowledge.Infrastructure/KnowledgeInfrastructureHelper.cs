using BrewUp.Knowledge.Core.Embeddings;
using BrewUp.Knowledge.Core.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Infrastructure;

public static class KnowledgeInfrastructureHelper
{
    public static IServiceCollection AddKnowledgeInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(AzureOpenAiEmbeddingOptions.SectionName)
            .Get<AzureOpenAiEmbeddingOptions>()
            ?? new AzureOpenAiEmbeddingOptions();

        services.AddSingleton(options);
        services.AddSingleton<IEmbeddingGenerator, AzureOpenAiEmbeddingGenerator>();
        services.AddSingleton<IKnowledgeIndex, InMemoryKnowledgeIndex>();

        return services;
    }
}
