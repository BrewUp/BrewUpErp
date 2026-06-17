namespace BrewUp.Shared.Agents;

public sealed record AgentResponse(
    string AgentName,
    string Summary,
    IReadOnlyDictionary<string, object?> Data,
    bool IsSuccessful = true);

public sealed record AgentResult(
    string AgentName,
    string CapabilityName,
    bool IsSuccessful,
    string Summary,
    IReadOnlyDictionary<string, object?> Data);
