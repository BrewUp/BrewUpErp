using BrewUp.Sales.ReadModel.Services;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.Messages.Events;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;

namespace BrewUp.Sales.ReadModel.Acl;

public sealed class CustomerDeletedEventHandler(ICustomerService customerService,
    ILoggerFactory loggerFactory)
    : IntegrationEventHandlerAsync<CustomerDeleted>(loggerFactory)
{
    public override async Task HandleAsync(CustomerDeleted @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await customerService.DeleteCustomerAsync((CustomerId)@event.AggregateId, cancellationToken);
    }
}