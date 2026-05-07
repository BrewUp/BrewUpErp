using BrewUp.Shared.DomainIds;
using BrewUp.Warehouse.ReadModel.Services;
using BrewUp.Warehouse.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;

namespace BrewUp.Warehouse.ReadModel.EventHandlers
{
    public sealed class WhAvailabilityCreatedEventHandler(
        IWhAvailabilityService availabilityService,
        ILoggerFactory loggerFactory) : DomainEventHandlerAsync<WhAvailabilityCreated>(loggerFactory)
    {
        public override async Task HandleAsync(WhAvailabilityCreated @event, CancellationToken cancellationToken = new())
        {
            cancellationToken.ThrowIfCancellationRequested();

            await availabilityService.AddWhAvailability(new AvailabilityId(@event.AggregateId.Value),
                @event.WarehouseId,
                @event.BeerId,
                @event.Quantity,
                cancellationToken);
        }
    }
}
