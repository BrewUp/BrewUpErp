using BrewUp.Sagas.Domain.Entities;
using BrewUp.Sagas.SharedKernel.CustomTypes;
using BrewUp.Sagas.SharedKernel.Messages.Commands;
using BrewUp.Sagas.SharedKernel.Messages.Events;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.Messages.Events.Sagas;
using Lena.Core;
using Muflone;
using Muflone.Messages;
using Muflone.Messages.Events;
using Muflone.Persistence;
using Muflone.Saga;

namespace BrewUp.Sagas.Domain.Orchestrators;

internal sealed class SalesOrderSagaOrchestrator(IRepository repository, 
    IEventBus eventBus) :
    ISalesOrderSagaOrchestrator,
    ISagaStartedByAsync<StartSalesOrderSaga>,
    IDomainEventHandlerAsync<SalesOrderSagaStarted>,
    IIntegrationEventHandlerAsync<CustomerBudgetVerified>,
    IIntegrationEventHandlerAsync<CustomerBudgetUnVerified>,
    IDomainEventHandlerAsync<SalesOrderSagaRejected>,
    IDomainEventHandlerAsync<SagaCustomerBudgetVerified>,
    IIntegrationEventHandlerAsync<SalesOrderPlaced>
{
    public async Task<Result<string>> StartSagaAsync(StartSalesOrderSaga command, CancellationToken cancellationToken)
    {
        await StartedByAsync(command).ConfigureAwait(false);
        
        return Result.Success(command.AggregateId.Value);
    }
    
    /// <summary>
    /// This is necessary to be able to start the saga from a command,
    /// as the ISagaStartedByAsync interface only has a StartSagaAsync method,
    /// but we want to have the possibility to start the saga from a command handler, and not from a domain event handler.
    /// With this operation, we initialized the saga's state.
    /// </summary>
    /// <param name="command"></param>
    public async Task StartedByAsync(StartSalesOrderSaga command)
    {
        Guid correlationId = Guid.CreateVersion7();
        
        var aggregate = SalesOrderSaga.Start(new SagaId(correlationId.ToString()),
            correlationId, command.SalesOrderNumber, command.SalesOrderDate, command.CustomerId,
            command.WarehouseId, command.SalesOrderDeliveryDate, command.Rows);
        await repository.SaveAsync(aggregate, Guid.CreateVersion7(), CancellationToken.None).ConfigureAwait(false);
    }
    
    public async Task HandleAsync(SalesOrderSagaStarted @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = MessageHelpers.GetCorrelationId(@event);
        var amountToCheck = @event.Rows.Sum(r => r.Quantity.Value * r.Price.Value);
        
        SalesOrderSagaStartedIntegrationEvent integrationEvent = new(new CustomerId(@event.CustomerId), correlationId, amountToCheck);
        await eventBus.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task HandleAsync(CustomerBudgetVerified @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = MessageHelpers.GetCorrelationId(@event);
        var aggregate = await repository
            .GetByIdAsync<SalesOrderSaga>(new SagaId(correlationId.ToString()), cancellationToken)
            .ConfigureAwait(false);
        aggregate!.MarkCustomerBudgetAsVerified(@event.Customer, correlationId);
        await repository.SaveAsync(aggregate, Guid.CreateVersion7(), cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleAsync(CustomerBudgetUnVerified @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = MessageHelpers.GetCorrelationId(@event);
        var aggregate = await repository
            .GetByIdAsync<SalesOrderSaga>(new SagaId(correlationId.ToString()), cancellationToken)
            .ConfigureAwait(false);
        aggregate!.MarkAsRejected(@event.Message, correlationId);
        
        await repository.SaveAsync(aggregate, Guid.CreateVersion7(), cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleAsync(RequestBeersAvailabilitySucceeded @event, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = MessageHelpers.GetCorrelationId(@event);
        var aggregate = await repository
            .GetByIdAsync<SalesOrderSaga>(new SagaId(correlationId.ToString()), cancellationToken)
            .ConfigureAwait(false);

        aggregate!.MarkOrderAvailable(correlationId);
    }

    public async Task HandleAsync(RequestBeersAvailabilityFailed @event, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = MessageHelpers.GetCorrelationId(@event);
        var aggregate = await repository
            .GetByIdAsync<SalesOrderSaga>(new SagaId(correlationId.ToString()), cancellationToken)
            .ConfigureAwait(false);

        aggregate!.MarkOrderNotAvailable(@event.Message, correlationId);

        await repository.SaveAsync(aggregate, Guid.CreateVersion7(), cancellationToken).ConfigureAwait(false);
    }

    public Task HandleAsync(SalesOrderSagaRejected message, CancellationToken cancellationToken = new ())
    {
        // Use signalR Hub to send response to the Client
        return Task.CompletedTask;
    }
    
    public async Task HandleAsync(SagaCustomerBudgetVerified @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = MessageHelpers.GetCorrelationId(@event);
        
        // IntegrationEvent for Sales
        SagaCustomerBudgetVerifiedIntegrationEvent integrationEvent = new(new IntegrationId(@event.AggregateId.Value),
            correlationId, @event.Order, @event.Customer);
        await eventBus.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task HandleAsync(SalesOrderPlaced @event, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = MessageHelpers.GetCorrelationId(@event);
        var aggregate = await repository
            .GetByIdAsync<SalesOrderSaga>(new SagaId(correlationId.ToString()), cancellationToken)
            .ConfigureAwait(false);
        aggregate!.MarkSalesOrderAsPlaced(correlationId);
        await repository.SaveAsync(aggregate, Guid.CreateVersion7(), cancellationToken).ConfigureAwait(false);
    }

    #region Dispose

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SalesOrderSagaOrchestrator()
    {
        Dispose(false);
    }

    #endregion
}