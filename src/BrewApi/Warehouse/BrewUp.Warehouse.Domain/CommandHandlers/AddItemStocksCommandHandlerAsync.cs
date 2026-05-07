using BrewUp.Warehouse.SharedKernel.Messages.Commands;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.Persistence;

namespace BrewUp.Warehouse.Domain.CommandHandlers
{
    internal sealed class AddItemStocksCommandHandlerAsync(IRepository repository,
        ILoggerFactory loggerFactory) : CommandHandlerAsync<AddItemStocks>(repository, loggerFactory)
    {
        public override async Task HandleAsync(AddItemStocks command, CancellationToken cancellationToken = default)
        {
            var aggregate = await Repository.GetByIdAsync<Entities.WhAvailability>(command.AggregateId, cancellationToken);


        }
    }
}
