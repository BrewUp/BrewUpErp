using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.Search;
using BrewUp.Knowledge.Facade.Ingestion;
using BrewUp.Knowledge.Facade.Search;
using BrewUp.Knowledge.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Facade;

public static class KnowledgeFacadeHelper
{
    public static IServiceCollection AddKnowledge(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IChunkingStrategy, SemanticChunkingStrategy>();
        services.AddScoped<IKnowledgeIngestionService, KnowledgeIngestionService>();
        services.AddScoped<IKnowledgeSearchEngine, KnowledgeSearchEngine>();
        services.AddScoped<IKnowledgeFacade, KnowledgeFacade>();
        services.AddKnowledgeInfrastructure(configuration);

        return services;
    }
}
