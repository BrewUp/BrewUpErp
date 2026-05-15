using BrewUp.AI.SharedKernel.Catalog;
using Lena.Core;

namespace BrewUp.AI.Facade.MasterData;

public interface IBeerCatalogQueriesFacade
{
    Task<Result<IReadOnlyCollection<BeerCatalogItem>>> GetCatalogBeersAsync(
        bool activeOnly,
        CancellationToken cancellationToken);
}
