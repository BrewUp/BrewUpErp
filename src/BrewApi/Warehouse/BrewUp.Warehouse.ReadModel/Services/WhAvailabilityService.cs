using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.ReadModel.Dtos;
using Lena.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public async Task<Result<bool>> AddWhAvailability(AvailabilityId availabilityId,
            WarehouseId warehouseId,
            BeerId beerId,
            Quantity quantity,
            CancellationToken cancellationToken)
        {
            var dto = WhAvailabilityDto.Create(availabilityId, warehouseId, beerId, quantity);

            return await Persister.InsertAsync(dto, cancellationToken);
        }
    }
}
