namespace BrewUp.Shared.Agents;

public interface IAgent
{
    string Name { get; }
    string SystemPrompt { get; }
    IReadOnlyCollection<AgentCapability> Capabilities { get; }
    bool CanHandle(string capabilityName);
    Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken);
}
