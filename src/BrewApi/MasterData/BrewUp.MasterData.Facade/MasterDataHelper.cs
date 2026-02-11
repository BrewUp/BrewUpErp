using System.Globalization;
using BrewUp.MasterData.Domain;
using BrewUp.MasterData.Infrastructure;
using BrewUp.MasterData.ReadModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.MasterData.Facade;

public static class MasterDataHelper
{
    public static IServiceCollection AddMasterDataFacade(this IServiceCollection services)
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
        
        services.AddScoped<ICustomerFacade, CustomerFacade>();
        services.AddScoped<IBeerFacade, BeerFacade>();

        services.AddMasterDataDomain();
        services.AddMasterDataInfrastructure();
        services.AddMasterDataReadModel();
        
        return services;
    }
}