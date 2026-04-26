using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;
using Muflone;
using Muflone.Messages.Events;

namespace BrewUp.Sales.ReadModel.EventHandlers;

public sealed class SalesOrderCreatedForIntegrationEventHandler(
    IEventBus  eventBus,
    ILoggerFactory loggerFactory) 
    : DomainEventHandlerAsync<SalesOrderCreated>(loggerFactory)
{
    public override async Task HandleAsync(SalesOrderCreated @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();

        await eventBus.PublishAsync(
            new SalesOrderCreatedIntegrationEvent(new SalesOrderId(@event.AggregateId.Value), @event.SalesOrderNumber.Value,
                @event.SalesOrderDate.Value, @event.Customer.CustomerId.Value, @event.Customer.CustomerName.Value, @event.SalesOrderDeliveryDate.Value,
                @event.Rows), cancellationToken);
    }
}