using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sagas;
using BrewUp.Shared.Messages.Commands.Sagas;
using Lena.Core;
using Muflone.Messages.Commands;

namespace BrewUp.Sagas.Facade;

internal sealed class SagasFacade(ICommandHandlerAsync<PlaceSalesOrder> placeSalesOrderCommandHandler) : ISagasFacade
{
    public async Task<Result<string>> PlaceSalesOrderAsync(PlaceSalesOrderJson body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        PlaceSalesOrder command = new(new IntegrationId(Guid.CreateVersion7().ToString()),
            Guid.CreateVersion7(),
            body.OrderNumber,
            body.OrderDate,
            body.CustomerId,
            body.DeliveryDate,
            body.Rows);
        
        await placeSalesOrderCommandHandler.HandleAsync(command, cancellationToken);
        
        return Result.Success(command.AggregateId.Value);
    }
}