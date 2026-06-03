namespace BrewUp.Mother.Facade.Agents;

public interface IAgent
{
    string Name { get; }
    IReadOnlyCollection<AgentCapability> Capabilities { get; }
    bool CanHandle(string capabilityName);
    Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken);
}
