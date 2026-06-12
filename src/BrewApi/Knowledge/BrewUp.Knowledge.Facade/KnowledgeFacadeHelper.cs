using System.Globalization;
using BrewUp.Knowledge.Core;
using BrewUp.Knowledge.Core.Chunking;
using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.ReadModel;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
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
        
        services.AddCore();
        services.AddInfrastructure(configuration);
        services.AddKnowledgeReadModel();

        return services;
    }
}
