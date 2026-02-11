using BrewUp.Sales.ReadModel.Dtos;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ReadModel;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.Services;

internal sealed class BeerService([FromKeyedServices("sales")] IPersister persister,
    ILoggerFactory loggerFactory) 
    : ServiceBase(persister, loggerFactory),IBeerService
{
    public async Task<Result<bool>> AddBeerAsync(BeerId beerId, BeerName beerName, BeerStyle style, AlcoholByVolume abv, Packaging packaging,
        Price price, bool isActive, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var beer = Beer.Create(beerId, beerName, style, abv, packaging, price, isActive);
        var insertResult = await Persister.InsertAsync(beer, cancellationToken);

        return insertResult.Match(
            _ => Result<bool>.Success(true),
            error =>
            {
                Logger.LogError("Error creating beer: {Error}", error);
                return Result<bool>.Error($"Error creating beer: {error}");
            });
    }
}