namespace BrewUp.Shared.Agents;

public sealed record AgentRequest(
    string Intent,
    string OriginalQuestion,
    IReadOnlyDictionary<string, object?> Inputs,
    Guid CorrelationId,
    AgentContext Context);
