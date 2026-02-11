using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

public interface IBeerFacade
{
    Task<Result<string>> CreateBeerAsync(CreateBeerJson body, CancellationToken cancellationToken);
}