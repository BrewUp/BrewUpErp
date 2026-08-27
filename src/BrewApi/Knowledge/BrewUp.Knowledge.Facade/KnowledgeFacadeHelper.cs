using System.Globalization;
using BrewUp.Knowledge.Core;
using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Facade.Agents;
using BrewUp.Knowledge.Facade.Evaluation;
using BrewUp.Knowledge.Facade.Governance;
using BrewUp.Knowledge.Facade.Wiki;
using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.ReadModel;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Shared;
using BrewUp.Shared.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Facade;

public static class KnowledgeFacadeHelper
{
    public static IServiceCollection AddKnowledgeFacade(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services.AddShared();
        services.AddValidation();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                if (context.ProblemDetails is HttpValidationProblemDetails validationProblemDetails)
                {
                    context.ProblemDetails.Detail =
                        $"Error(s) occurred: {validationProblemDetails.Errors.Values.Sum(x => x.Length)}";
                }

                context.ProblemDetails.Extensions.TryAdd("timestamp",
                    DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            };
        });

        services.AddScoped<IKnowledgeFacade, KnowledgeFacade>();
        services.AddScoped<KnowledgeAgent>();
        services.AddScoped<IAgent>(sp => sp.GetRequiredService<KnowledgeAgent>());
        services.AddScoped<IAgentCardProvider, KnowledgeAgentCardProvider>();
        services.AddScoped<GetKnowledgeDocumentsHandler>();
        services.AddScoped<GetKnowledgeDocumentHandler>();
        services.AddScoped<DeleteKnowledgeDocumentHandler>();
        services.AddScoped<ReindexKnowledgeDocumentHandler>();
        services.AddScoped<KnowledgeRetrievalEvaluator>();
        services.AddHostedService<WikiSynthesisWorker>();

        services.AddCore();
        services.AddInfrastructure(configuration);
        services.AddKnowledgeReadModel();

        return services;
    }
}
