using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.ReadModel.Services;
using BrewUp.Warehouse.SharedKernel.Messages.Commands;
using Lena.Core;
using Muflone.Messages.Commands;

namespace BrewUp.Warehouse.Facade;

internal sealed class WarehouseFacade(IShipmentService shipmentService, 
    IWarehouseService warehouseService,
    ICommandHandlerAsync<AddItemStocks> commandHandler) : IWarehouseFacade
{
    public Task<Result<PagedResult<ShipmentJson>>> GetShipmentOrdersAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken) =>
        shipmentService.GetShipmentsAsync(pageNumber, pageSize, cancellationToken);

    public async Task<Result<string>> AddItemStocksAsync(WarehouseJson warehouseJson, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var warehouseResult = await warehouseService.GetWarehouseByIdAsync(new WarehouseId(warehouseJson.Id), cancellationToken);
        if (warehouseResult.IsError)
            return Result<string>.Error("Warehouse not found");

        AddItemStocks command = new(new WarehouseId(warehouseJson.Id), 
            warehouseJson.ItemStocks, 
            Guid.NewGuid());

        await commandHandler.HandleAsync(command, cancellationToken);

        return Result.Success("OK");
    }
}