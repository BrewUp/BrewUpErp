using System.Globalization;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace BrewUp.Rest.Module;

public class OpenApiModule : IModule
{
  public bool IsEnabled => true;
  public int Order => 0;

  public IServiceCollection Register(WebApplicationBuilder builder)
  {
    builder.Services.AddOpenApi(options =>
    {
      options.AddDocumentTransformer((document, _, _) =>
      {
        document.Servers = [new OpenApiServer { Url = "/" }];
        document.Info = new OpenApiInfo
        {
          Title = "BrewUp API",
          Version = "v1.0",
          Description = "BrewUp API",
          Contact = new OpenApiContact
          {
            Name = "BrewUp Team",
          }
        };

        return Task.CompletedTask;
      });
    });
    
    builder.Services.AddValidation();
    builder.Services.AddProblemDetails(options =>
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
    
    return builder.Services;
  }

  public WebApplication Configure(WebApplication app)
  {
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
      options.WithTitle("BrewUp API")
        .WithTheme(ScalarTheme.None);
    });

    return app;
  }
}