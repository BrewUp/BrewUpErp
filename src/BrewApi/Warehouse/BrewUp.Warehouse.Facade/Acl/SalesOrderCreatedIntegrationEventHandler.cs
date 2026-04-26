using BrewUp.Warehouse.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;

namespace BrewUp.Warehouse.Facade.Acl;

public sealed class SalesOrderCreatedIntegrationEventHandler(ILoggerFactory loggerFactory)
    : IntegrationEventHandlerAsync<SalesOrderCreatedIntegrationEvent>(loggerFactory)
{
    public override Task HandleAsync(SalesOrderCreatedIntegrationEvent @event,
        CancellationToken cancellationToken = new ())
    {
        throw new NotImplementedException();
    }
}