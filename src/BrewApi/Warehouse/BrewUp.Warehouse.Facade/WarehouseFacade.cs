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
    ICommandHandlerAsync<AddItemStock> commandHandler) : IWarehouseFacade
{
    public Task<Result<PagedResult<ShipmentJson>>> GetShipmentOrdersAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken) =>
        shipmentService.GetShipmentsAsync(pageNumber, pageSize, cancellationToken);

    public async Task<Result<string>> AddItemStockAsync(AddItemStockJson json, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var warehouseResult = await warehouseService.GetWarehouseByIdAsync(new WarehouseId(json.WarehouseId), cancellationToken);
        if (warehouseResult.IsError)
            return Result<string>.Error("Warehouse not found");

        AddItemStock command = new(new AvailabilityId(json.Id), 
            new Shared.CustomTypes.Quantity(json.Quantity, json.UnitOfMeasure), 
            Guid.NewGuid());

        await commandHandler.HandleAsync(command, cancellationToken);

        return Result.Success("OK");
    }
}