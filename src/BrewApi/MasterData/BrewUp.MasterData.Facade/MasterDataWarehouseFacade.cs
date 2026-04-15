using BrewUp.MasterData.Domain.Services;
using BrewUp.Shared.ExternalContracts.Warehouse;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

internal class MasterDataWarehouseFacade(IWarehouseDomainService warehouseDomainService) : IMasterDataWarehouseFacade
{
    public Task<Result<string>> CreateWarehouseAsync(CreateWarehouseJson body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        return warehouseDomainService.CreateWarehouseAsync(body, cancellationToken);
    }
}