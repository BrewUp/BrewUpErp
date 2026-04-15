using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using Lena.Core;

namespace BrewUp.Warehouse.ReadModel.Services;

public interface IWarehouseService
{
    Task<Result<bool>> AddWarehouseAsync(WarehouseId warehouseId, WarehouseName warehouseName,
        CancellationToken cancellationToken = default);
}