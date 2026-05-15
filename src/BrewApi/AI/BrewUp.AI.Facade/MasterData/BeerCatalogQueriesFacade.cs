using BrewUp.AI.SharedKernel.Catalog;
using BrewUp.MasterData.ReadModel.Services;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using Lena.Core;

namespace BrewUp.AI.Facade.MasterData;

internal sealed class BeerCatalogQueriesFacade(
    IBeerQueryService beerQueryService) : IBeerCatalogQueriesFacade
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 250;
    
    public async Task<Result<IReadOnlyCollection<BeerCatalogItem>>> GetCatalogBeersAsync(
        bool activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await beerQueryService.GetBeersAsync(
            DefaultPageNumber,
            DefaultPageSize,
            cancellationToken);

        if (!result.IsSuccess)
            return Result<IReadOnlyCollection<BeerCatalogItem>>.Error("No Beers Found");

        result.TryGetValue(out Shared.ReadModel.PagedResult<BeerJson> page);
        
        return Result<IReadOnlyCollection<BeerCatalogItem>>.Success(
            page.Results
                .Where(beer => !activeOnly || beer.IsActive)
                .Select(Map)
                .ToArray()
            );
    }
    
    private static BeerCatalogItem Map(BeerJson beer) =>
        new(
            BeerId: beer.BeerId,
            Name: beer.BeerName,
            Style: beer.BeerStyle,
            AlcoholByVolume: beer.AlcoholByVolume,
            IsActive: beer.IsActive);
}