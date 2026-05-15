using BrewUp.MasterData.ReadModel.Services;
using BrewUp.Mcp.SharedKernel.Catalog;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;

namespace BrewUp.Mcp.Facade.MasterData;

internal sealed class BeerCatalogQueriesFacade(
    IBeerQueryService beerQueryService) : IBeerCatalogQueriesFacade
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 250;
    
    public async Task<IReadOnlyCollection<BeerCatalogItem>> GetCatalogBeersAsync(
        bool activeOnly,
        CancellationToken cancellationToken)
    {
        var result = await beerQueryService.GetBeersAsync(
            DefaultPageNumber,
            DefaultPageSize,
            cancellationToken);

        if (!result.IsSuccess)
            return [];

        result.TryGetValue(out Shared.ReadModel.PagedResult<BeerJson> page);
        
        return page.Results.Select(Map).ToArray();
    }
    
    private static BeerCatalogItem Map(BeerJson beer) =>
        new(
            BeerId: beer.BeerId,
            Name: beer.BeerName,
            Style: beer.BeerStyle,
            AlcoholByVolume: beer.AlcoholByVolume,
            IsActive: beer.IsActive);
}