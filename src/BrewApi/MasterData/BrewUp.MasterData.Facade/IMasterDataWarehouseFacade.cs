using BrewUp.Shared.ExternalContracts.Warehouse;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

public interface IMasterDataWarehouseFacade
{
    Task<Result<string>> CreateWarehouseAsync(CreateWarehouseJson body, CancellationToken cancellationToken);
}