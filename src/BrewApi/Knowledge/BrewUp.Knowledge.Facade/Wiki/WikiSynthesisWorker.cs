using BrewUp.Knowledge.Core.Wiki;
using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.SharedKernel.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BrewUp.Knowledge.Facade.Wiki;

internal sealed class WikiSynthesisWorker(
    IServiceScopeFactory scopeFactory,
    WikiOptions options,
    ILogger<WikiSynthesisWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("Knowledge Wiki synthesis is disabled.");
            return;
        }

        logger.LogInformation("Knowledge Wiki synthesis worker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<WikiSynthesisService>();
                var processed = await service.ProcessNextAsync(stoppingToken);
                if (processed)
                    continue;

                var repository = scope.ServiceProvider.GetRequiredService<IWikiRepository>();
                if (await repository.EnqueueMissingDocumentsAsync(stoppingToken) > 0)
                    continue;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Knowledge Wiki synthesis polling failed.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(Math.Max(1, options.PollIntervalSeconds)),
                stoppingToken);
        }
    }
}
