using BrewUp.Shared.DomainIds;
using Muflone.Messages.Events;

namespace BrewUp.Shared.Messages.Events.Sagas;

public sealed class RequestBeersAvailabilityFailed(WarehouseId aggregateId, Guid correlationId,
    string message) : IntegrationEvent(aggregateId, correlationId)
{
    public string Message { get; private set; } = message;
}