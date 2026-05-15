using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Warehouse.SharedKernel.CustomTypes;

namespace BrewUp.Warehouse.McpServer;

internal sealed class McpWarehouseFacade : IMcpWarehouseFacade
{
    public Task<AvailabilityJson> GetBeerAvailabilityAsync(string beerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotImplementedException();
    }

    public Task<ReorderThresold> GetReorderThresholdAsync(string beerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotImplementedException();
    }
}