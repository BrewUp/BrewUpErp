using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Warehouse;
using Lena.Core;

namespace BrewUp.Warehouse.ReadModel.Services
{
    public interface IWhAvailabilityService
    {
        Task<Result<bool>> AddWhAvailability(AvailabilityId availabilityId, WarehouseId warehouseId, BeerId beerId, Quantity quantity, CancellationToken cancellationToken);
        Task<Result<WhAvailabilityJson>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}
