using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Core.Wiki;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Core;

public static class KnowledgeCoreHelper
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IChunkingStrategy, SemanticChunkingStrategy>();
        services.AddScoped<IngestKnowledgeDocumentHandler>();
        services.AddScoped<WikiAnalysisValidator>();
        services.AddScoped<WikiSynthesisService>();
        services.AddSingleton<IChunkingPolicy, DefaultChunkingPolicy>();
        
        return services;
    }
}