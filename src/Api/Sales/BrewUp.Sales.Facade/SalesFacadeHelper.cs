using BrewUp.Sales.Domain;
using BrewUp.Sales.Facade.Validators;
using BrewUp.Sales.Infrastructure;
using BrewUp.Sales.ReadModel;
using BrewUp.Shared.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.Facade;

public static class SalesFacadeHelper
{
    public static IServiceCollection AddSalesFacade(this IServiceCollection services,
        ILoggerFactory loggerFactory,
        IConfigurationManager configurationManager)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateSalesOrderValidator>();
        services.AddSingleton<ValidationHandler>();
        
        services.AddScoped<ISalesFacade, SalesFacade>();

        services.AddSalesDomain();
        services.AddSalesReadModel();
        services.AddSalesInfrastructure(loggerFactory, configurationManager);

        return services;
    }
}