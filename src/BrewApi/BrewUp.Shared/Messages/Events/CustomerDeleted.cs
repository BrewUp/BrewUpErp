using BrewUp.Shared.DomainIds;
using Muflone.Messages.Events;

namespace BrewUp.Shared.Messages.Events;

public sealed class CustomerDeleted(CustomerId aggregateId) : IntegrationEvent(aggregateId)
{
}