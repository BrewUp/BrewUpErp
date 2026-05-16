using BrewUp.MasterData.ReadModel.Services;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;

namespace BrewUp.MasterData.McpServer;

internal sealed class McpMasterDataFacade(
    IBeerQueryService beerQueryService
    ) : IMcpMasterDataFacade
{
    public async Task<IReadOnlyCollection<BeerJson>> GetBeersCatalogAsync(CancellationToken cancellationToken)
    {
        var beersResult = await beerQueryService.GetBeersAsync(1, int.MaxValue, cancellationToken);
        if (beersResult.IsError)
            return [];
        
        return beersResult.TryGetValue(out var beers) 
            ? beers.Results.ToList() 
            : [];
    }
}