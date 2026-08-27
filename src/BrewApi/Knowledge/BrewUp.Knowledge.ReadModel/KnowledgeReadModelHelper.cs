using BrewUp.Knowledge.ReadModel.QueryHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.ReadModel;

public static class KnowledgeReadModelHelper
{
    public static IServiceCollection AddKnowledgeReadModel(this IServiceCollection services)
    {
        services.AddScoped<GetKnowledgeDocumentChunksHandler>();
        services.AddScoped<SearchKnowledgeHandler>();
        services.AddScoped<QueryWikiHandler>();
        services.AddScoped<GetWikiPageHandler>();
        services.AddScoped<GetWikiPageEvidenceHandler>();
        services.AddScoped<GetWikiProcessingJobHandler>();

        return services;
    }
}
