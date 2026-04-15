using BrewUp.MasterData.Domain.Services;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

internal sealed class BeerFacade(IBeerDomainService beerDomainService) : IMasterDataBeerFacade
{
    public Task<Result<string>> CreateBeerAsync(CreateBeerJson body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return beerDomainService.CreateBeerAsync(new BeerId(Guid.CreateVersion7().ToString()),
            new BeerName(body.BeerName),
            new BeerStyle(body.BeerStyle),
            new AlcoholByVolume(body.AlcoholByVolume),
            new Packaging(body.Packaging),
            new Price(body.Price.Value, body.Price.Currency),
            body.IsActive, cancellationToken);
    }
}