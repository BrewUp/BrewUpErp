using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.ReadModel.Services;
using Lena.Core;

namespace BrewUp.Warehouse.Facade;

internal sealed class WarehouseFacade(IShipmentService shipmentService) : IWarehouseFacade
{
    public Task<Result<PagedResult<ShipmentJson>>> GetShipmentOrdersAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken) =>
        shipmentService.GetShipmentsAsync(pageNumber, pageSize, cancellationToken);
}