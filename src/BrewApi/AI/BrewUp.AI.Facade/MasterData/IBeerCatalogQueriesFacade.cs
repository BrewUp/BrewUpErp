using BrewUp.AI.SharedKernel.Catalog;

namespace BrewUp.AI.Facade.MasterData;

public interface IBeerCatalogQueriesFacade
{
    Task<IReadOnlyCollection<BeerCatalogItem>> GetCatalogBeersAsync(
        bool activeOnly,
        CancellationToken cancellationToken);
}
