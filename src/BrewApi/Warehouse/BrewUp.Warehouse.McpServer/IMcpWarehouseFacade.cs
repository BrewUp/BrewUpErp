using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Warehouse.SharedKernel.CustomTypes;

namespace BrewUp.Warehouse.McpServer;

public interface IMcpWarehouseFacade
{
    Task<AvailabilityJson> GetBeerAvailabilityAsync(string beerId, CancellationToken cancellationToken);
    Task<ReorderThresold> GetReorderThresholdAsync(string beerId, CancellationToken cancellationToken);
}