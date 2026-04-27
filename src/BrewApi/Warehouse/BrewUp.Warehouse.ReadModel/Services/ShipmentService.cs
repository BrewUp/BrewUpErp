using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.SharedKernel.CustomTypes;
using BrewUp.Warehouse.SharedKernel.Enums;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrewUp.Warehouse.ReadModel.Services;

internal sealed class ShipmentService([FromKeyedServices("warehouse")] IPersister persister,
    ILoggerFactory loggerFactory) 
    : ServiceBase(persister, loggerFactory), IShipmentService
{
    public async Task<Result<bool>> AddShipmentAsync(ShipmentId shipmentId, SalesOrderId salesOrderId, CustomerId customerId,
        DeliveryDate deliveryDate, IEnumerable<OrderRowDto> rows, ShipmentState shipmentState, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var shipment = Dtos.Shipment.Create(shipmentId, salesOrderId, customerId, deliveryDate, shipmentState, rows);
        var insertResult = await Persister.InsertAsync(shipment, cancellationToken);
        
        return insertResult.Match(
            _ => Result<bool>.Success(true),
            error =>
            {
                Logger.LogError("Error creating shipment: {Error}", error);
                return Result<bool>.Error($"Error creating shipment: {error}");
            });
    }
}