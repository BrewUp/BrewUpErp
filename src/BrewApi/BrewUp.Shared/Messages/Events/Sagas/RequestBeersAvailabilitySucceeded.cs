using BrewUp.Shared.DomainIds;
using Muflone.Messages.Events;

namespace BrewUp.Shared.Messages.Events.Sagas;

public sealed class RequestBeersAvailabilitySucceeded(WarehouseId aggregateId, Guid correlationId) : IntegrationEvent(aggregateId, correlationId)
{
}