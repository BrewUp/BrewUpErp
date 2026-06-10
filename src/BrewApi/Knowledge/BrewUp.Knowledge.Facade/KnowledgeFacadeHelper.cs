using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Facade.Ingestion;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Facade;

public static class KnowledgeFacadeHelper
{
    public static IServiceCollection AddKnowledgeFacade(this IServiceCollection services)
    {
        services.AddSingleton<IChunkingStrategy, SemanticChunkingStrategy>();
        services.AddScoped<IngestKnowledgeDocumentHandler>();
        services.AddScoped<IKnowledgeFacade, KnowledgeFacade>();

        return services;
    }
}
