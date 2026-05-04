using BrewUp.Sagas.Domain.Entities;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.Messages.Commands.Sagas;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.Persistence;

namespace BrewUp.Sagas.Domain.CommandHandlers;

internal sealed class PlaceSalesOrderCommandHandler(IRepository repository,
    ILoggerFactory loggerFactory) : CommandHandlerAsync<PlaceSalesOrder>(repository, loggerFactory)
{
    public override async Task HandleAsync(PlaceSalesOrder command, CancellationToken cancellationToken = new ())
    {
        var aggregate = SalesOrderSaga.Create(new IntegrationId(command.AggregateId.Value),
            command.CorrelationId, command.SalesOrderNumber, command.SalesOrderDate, command.CustomerId,
            command.SalesOrderDeliveryDate, command.Rows);
        await Repository.SaveAsync(aggregate, Guid.CreateVersion7(), cancellationToken).ConfigureAwait(false);
    }
}