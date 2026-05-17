using BrewUp.Chat.SharedKernel.Catalog;

namespace BrewUp.Chat.Facade.MasterData;

public interface IBeerCatalogQueriesFacade
{
    Task<IReadOnlyCollection<BeerCatalogItem>> GetCatalogBeersAsync(
        bool activeOnly,
        CancellationToken cancellationToken);
}
