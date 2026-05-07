using BrewUp.Shared.DomainIds;
using BrewUp.Warehouse.Domain.Entities;
using BrewUp.Warehouse.SharedKernel.Messages.Commands;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.Persistence;

namespace BrewUp.Warehouse.Domain.CommandHandlers
{
    public sealed class CreateWhAvailabilityCommandHandler(IRepository repository,
        ILoggerFactory loggerFactory) : CommandHandlerAsync<CreateWhAvailability>(repository, loggerFactory)
    {
        public override async Task HandleAsync(CreateWhAvailability command, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var aggregate = WhAvailability.Create(
                new AvailabilityId(command.AggregateId.Value),
                command.WarehouseId,
                command.BeerId,
                new Quantity(command.Quantity.Value, command.Quantity.UnitOfMeasure));

            await Repository.SaveAsync(aggregate, Guid.NewGuid(), cancellationToken);
        }
    }
}
