using BrewUp.Mcp.SharedKernel.Catalog;

namespace BrewUp.Mcp.Facade.MasterData;

public interface IBeerCatalogQueriesFacade
{
    Task<IReadOnlyCollection<BeerCatalogItem>> GetCatalogBeersAsync(
        bool activeOnly,
        CancellationToken cancellationToken);
}
