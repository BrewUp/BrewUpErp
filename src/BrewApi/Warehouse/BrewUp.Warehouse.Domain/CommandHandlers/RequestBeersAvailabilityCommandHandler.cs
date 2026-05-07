using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.Messages.Events.Sagas;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.Entities.Dtos;
using BrewUp.Warehouse.SharedKernel.CustomTypes;
using BrewUp.Warehouse.SharedKernel.Messages.Commands;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone;

namespace BrewUp.Warehouse.Domain.CommandHandlers
{
        /*
    public sealed class RequestBeersAvailabilityCommandHandler([FromKeyedServices("warehouse")] IPersister persister,
        IEventBus eventBus,
        ILoggerFactory loggerFactory) : WarehouseCommandHandlerAsync<RequestBeersAvailability>(persister, loggerFactory)
    {
        public override async Task HandleAsync(RequestBeersAvailability command, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var warehouseResult = await Persister.GetByIdAsync<WarehouseDto>(command.AggregateId.Value, cancellationToken);
            if (!warehouseResult.IsSuccess)
                return;

            warehouseResult.TryGetValue(out WarehouseDto warehouse);

            var warehouseId = new WarehouseId(command.AggregateId.Value);
            var correlationId = command.MessageId;

            var warehouseEntity = Entities.WhAvailability.Create(warehouseId, warehouse.Name, warehouse.ItemStocks, correlationId);

            var result = await TryRemoveBeersAndSave(warehouseEntity, command.Rows.ToList(), cancellationToken);

            if (result.IsSuccess)
            {
                RequestBeersAvailabilitySucceeded requestSucceeded = new(warehouseId, correlationId);
                await eventBus.PublishAsync(requestSucceeded, cancellationToken);

                return;
            }

            RequestBeersAvailabilityFailed requestFailed = new(warehouseId, correlationId, ""); //DEVNOTE: how can I access the error message from result?
            await eventBus.PublishAsync(requestFailed, cancellationToken);
        }

        private async Task<Result<bool>> TryRemoveBeersAndSave(Entities.WhAvailability warehouse, List<ItemRequest> itemRequests, CancellationToken cancellationToken)
        {
            if (!TryRemoveBeersFromWarehouse(warehouse, itemRequests))
                return Result<bool>.Error("Not enough beers");

            var dto = WarehouseDto.Create(
                (WarehouseId)warehouse.Id,
                new WarehouseName(warehouse.Name),
                warehouse.ItemStocks.Select(s => new ItemStock()
                {
                    BeerId = s.Value.BeerId.Value,
                    Quantity = s.Value.Quantity.ToString()
                }).ToList());

            return (await Persister.UpdateAsync(dto, cancellationToken))
                .Match(
                    _ => Result<bool>.Success(true),
                    Result<bool>.Error);
        }

        private static bool TryRemoveBeersFromWarehouse(Entities.WhAvailability warehouse, List<ItemRequest> itemRequests)
        {
            foreach (var itemRequest in itemRequests)
            {
                if (warehouse.ItemStocks.TryGetValue(new BeerId(itemRequest.BeerId.Value), out var itemStock))
                {
                    if (!itemStock.TryRemoveItems(itemRequest.Quantity))
                        return false;
                }
                else
                    return false;
            }
            return true;
        }
    }*/
}
