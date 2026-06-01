using Muflone.Messages.Events;

namespace BrewUp.Shared.Agents;

public abstract class AgentBase<TInput, TOutput>
    where TInput : IntegrationEvent
    where TOutput : IntegrationEvent
{
    public abstract Task<TOutput> HandleAsync(
        TInput @event,
        CancellationToken cancellationToken = default);
}