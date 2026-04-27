using BrewUp.Shared.Messages.Events;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.Persistence;

namespace BrewUp.Dashboards.Facade.Acl;

public sealed class SalesOrderCreatedIntegrationForCustomerSummaryEventHandler(IServiceBus serviceBus, 
    ILoggerFactory loggerFactory)
    : IntegrationEventHandlerAsync<SalesOrderCreatedIntegrationEvent>(loggerFactory)
{
    public override async Task HandleAsync(SalesOrderCreatedIntegrationEvent @event,
        CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // PrepareShipment command = new(new ShipmentId(Guid.NewGuid().ToString()),
        //     new SalesOrderId(@event.AggregateId.Value),
        //     new CustomerId(@event.CustomerId),
        //     new DeliveryDate(@event.SalesOrderDeliveryDate),
        //     @event.Rows,
        //     @event.MessageId);
        //
        // await serviceBus.SendAsync(command, cancellationToken);
    }
}