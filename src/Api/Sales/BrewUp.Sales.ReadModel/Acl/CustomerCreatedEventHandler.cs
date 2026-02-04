using BrewUp.Sales.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;

namespace BrewUp.Sales.ReadModel.Acl;

public sealed class CustomerCreatedEventHandler(ILoggerFactory loggerFactory)
    : IntegrationEventHandlerAsync<CustomerCreated>(loggerFactory)
{
    public override Task HandleAsync(CustomerCreated @event, CancellationToken cancellationToken = new ())
    {
        throw new NotImplementedException();
    }
}