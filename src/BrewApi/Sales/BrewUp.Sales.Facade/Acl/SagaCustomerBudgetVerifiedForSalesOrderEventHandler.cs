using BrewUp.Sales.SharedKernel.Messages.Commands;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.Messages.Events.Sagas;
using Microsoft.Extensions.Logging;
using Muflone.Messages;
using Muflone.Messages.Events;
using Muflone.Persistence;

namespace BrewUp.Sales.Facade.Acl;

public sealed class SagaCustomerBudgetVerifiedForSalesOrderEventHandler(IServiceBus serviceBus,
    ILoggerFactory loggerFactory)
: IntegrationEventHandlerAsync<SagaCustomerBudgetVerifiedForSalesOrder>(loggerFactory)
{
    public override async Task HandleAsync(SagaCustomerBudgetVerifiedForSalesOrder @event,
        CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        PlaceSalesOrder command = new(new IntegrationId(@event.AggregateId.Value), MessageHelpers.GetCorrelationId(@event), 
            @event.SalesOrder, @event.Customer);
        await serviceBus.SendAsync(command, cancellationToken).ConfigureAwait(false);
    }
}