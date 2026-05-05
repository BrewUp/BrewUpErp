using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.MasterData.Customers;
using Muflone.Messages.Events;

namespace BrewUp.Sagas.SharedKernel.Messages.Events;

public sealed class SagaCustomerBudgetVerified(CustomerId aggregateId, Guid correlationId,
    CustomerJson customer) : DomainEvent(aggregateId, correlationId)
{
    public CustomerJson Customer { get; private set; } = customer;
}