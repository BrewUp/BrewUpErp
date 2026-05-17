using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Warehouse.ReadModel.Services;
using BrewUp.Warehouse.SharedKernel.CustomTypes;

namespace BrewUp.Warehouse.McpServer;

internal sealed class McpWarehouseFacade(
    IAvailabilityService availabilityService
    ) : IMcpWarehouseFacade
{
    public async Task<AvailabilityWithThresholdJson> GetBeerAvailabilityAsync(string beerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var availabilityResult =
            await availabilityService.GetAvailabilityByBeerIdAsync(new BeerId(beerId), cancellationToken);
        
        if (availabilityResult.IsError)
            return new AvailabilityWithThresholdJson();
        
        return availabilityResult.TryGetValue(out var availability) 
            ? new AvailabilityWithThresholdJson
            {
                Id = availability.Id,
                WarehouseId = availability.WarehouseId,
                BeerId = availability.BeerId,
                Quantity = availability.Quantity,
                ReorderThreshold = 300,
                UnitOfMeasure = availability.UnitOfMeasure
            }
            : new AvailabilityWithThresholdJson();
    }

    public async Task<ReorderThreshold> GetReorderThresholdAsync(string beerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var thresholdResult =
            await availabilityService.GetReorderThresholdByBeerIdAsync(new BeerId(beerId), cancellationToken);
        
        if (thresholdResult.IsError)
            return new ReorderThreshold(new BeerId(beerId),  new ThresholdQuantity(0, "Bottle"));
        
        return thresholdResult.TryGetValue(out var threshold)
            ? threshold
            : new ReorderThreshold(new BeerId(beerId),  new ThresholdQuantity(0, "Bottle"));
    }
}