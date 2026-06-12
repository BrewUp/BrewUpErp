using BrewUp.Knowledge.ReadModel.QueryHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.ReadModel;

public static class KnowledgeReadModelHelper
{
    public static IServiceCollection AddKnowledgeReadModel(this IServiceCollection services)
    {
        services.AddScoped<GetKnowledgeDocumentChunksHandler>();
        services.AddScoped<SearchKnowledgeHandler>();

        return services;
    }
}
