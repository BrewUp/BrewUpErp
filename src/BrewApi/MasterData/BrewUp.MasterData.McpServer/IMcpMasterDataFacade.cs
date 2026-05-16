using BrewUp.Shared.ExternalContracts.MasterData.Beers;

namespace BrewUp.MasterData.McpServer;

public interface IMcpMasterDataFacade
{
    Task<IReadOnlyCollection<BeerJson>> GetBeersCatalogAsync(CancellationToken cancellationToken);
}