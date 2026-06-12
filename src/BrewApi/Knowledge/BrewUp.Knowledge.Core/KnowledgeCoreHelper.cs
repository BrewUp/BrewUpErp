using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Core;

public static class KnowledgeCoreHelper
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<IChunkingStrategy, SemanticChunkingStrategy>();
        services.AddScoped<IngestKnowledgeDocumentHandler>();
        services.AddSingleton<IChunkingPolicy, DefaultChunkingPolicy>();
        
        return services;
    }
}