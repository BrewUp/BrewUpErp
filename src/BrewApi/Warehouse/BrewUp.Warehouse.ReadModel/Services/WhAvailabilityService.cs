using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.ReadModel.Dtos;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrewUp.Warehouse.ReadModel.Services
{
    internal class WhAvailabilityService([FromKeyedServices("warehouse")] IPersister persister,
    IQueries<WhAvailabilityDto> queries,
    ILoggerFactory loggerFactory)
    : ServiceBase(persister, loggerFactory), IWhAvailabilityService
    {
        public async Task<Result<WhAvailabilityJson>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var queryResult = await queries.GetByIdAsync(id, cancellationToken);

            if (queryResult.IsSuccess)
            {
                queryResult.TryGetValue(out WhAvailabilityDto availabilityDto);
                return Result<WhAvailabilityJson>.Success(availabilityDto.ToJson());
            }
            return Result<WhAvailabilityJson>.Error("WhAvailability not found");
        }

        public async Task<Result<bool>> AddWhAvailabilityAsync(AvailabilityId availabilityId,
            WarehouseId warehouseId,
            BeerId beerId,
            Quantity quantity,
            CancellationToken cancellationToken)
        {
            var dto = WhAvailabilityDto.Create(availabilityId, warehouseId, beerId, quantity);

            return await Persister.InsertAsync(dto, cancellationToken);
        }

        public async Task<Result<string>> AddItemStockAsync(AvailabilityId availabilityId,
            Quantity quantity,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var persisterResult = await Persister.GetByIdAsync<WhAvailabilityDto>(availabilityId.Value, cancellationToken);
            if (!persisterResult.IsSuccess)
                return Result<string>.Error("Error retrieving warehouse availability");

            persisterResult.TryGetValue(out WhAvailabilityDto availabilityDto);
            availabilityDto.UpdateQuantity(quantity);

            var updateResult = await Persister.UpdateAsync(availabilityDto, cancellationToken);
            return updateResult.Match(
                _ => Result<string>.Success(availabilityId.Value),
                _ => Result<string>.Error("Error updating warehouse availability"));
        }
    }
}
