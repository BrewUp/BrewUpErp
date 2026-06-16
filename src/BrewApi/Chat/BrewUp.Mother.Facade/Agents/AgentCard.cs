namespace BrewUp.Mother.Facade.Agents;

public sealed record AgentCard(
    string Name,
    string Description,
    string Version,
    IReadOnlyCollection<AgentSkill> Skills,
    IReadOnlyCollection<AgentCapability> Capabilities);

public sealed record AgentSkill(
    string Name,
    string Description);
