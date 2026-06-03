namespace BrewUp.Mother.Facade.Agents;

public sealed record AgentResponse(
    string AgentName,
    string Summary,
    IReadOnlyDictionary<string, object?> Data,
    bool IsSuccessful = true);
