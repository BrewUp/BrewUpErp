using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using BrewUp.Shared.ExternalContracts.MasterData.Customers;

namespace BrewUp.MasterData.McpServer;

public interface IMcpMasterDataFacade
{
    Task<IReadOnlyCollection<BeerJson>> GetBeersCatalogAsync(CancellationToken cancellationToken);
    Task<BeerJson> GetBeerDetailsAsync(string beerId, CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<CustomerJson>> GetActiveCustomersAsync(CancellationToken cancellationToken);
    Task<CustomerJson> GetCustomerInfoAsync(string customerId, CancellationToken cancellationToken);
}