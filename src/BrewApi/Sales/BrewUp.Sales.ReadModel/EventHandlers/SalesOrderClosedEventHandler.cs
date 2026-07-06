using BrewUp.Sales.ReadModel.Services;
using BrewUp.Sales.SharedKernel.Messages.Events;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;

namespace BrewUp.Sales.ReadModel.EventHandlers;

public sealed class SalesOrderClosedEventHandler(
    ISalesOrderService salesOrderService,
    ILoggerFactory loggerFactory) 
    : DomainEventHandlerAsync<SalesOrderClosed>(loggerFactory)
{
    public override Task HandleAsync(SalesOrderClosed @event, CancellationToken cancellationToken = new ())
    {
        return Task.CompletedTask;
    }
}