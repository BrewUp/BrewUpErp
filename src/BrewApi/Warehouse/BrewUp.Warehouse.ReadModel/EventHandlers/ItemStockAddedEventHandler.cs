using BrewUp.Shared.DomainIds;
using BrewUp.Warehouse.ReadModel.Services;
using BrewUp.Warehouse.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;

namespace BrewUp.Warehouse.ReadModel.EventHandlers;

public sealed class ItemStockAddedEventHandler(
    IWhAvailabilityService availabilityService,
    ILoggerFactory loggerFactory) : DomainEventHandlerAsync<WhAvailabilityCreated>(loggerFactory)
{
    public override async Task HandleAsync(WhAvailabilityCreated @event, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await availabilityService.AddItemStockAsync(new AvailabilityId(@event.AggregateId.Value), @event.Quantity, cancellationToken);
    }
}
