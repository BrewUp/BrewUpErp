namespace BrewUp.Mother.Facade.Agents;

public sealed record AgentRequest(
    string Intent,
    string OriginalQuestion,
    IReadOnlyDictionary<string, object?> Inputs,
    Guid CorrelationId,
    AgentContext Context);

public sealed record AgentContext(
    string ConversationId,
    string RequestedBy,
    IReadOnlyCollection<string> InvokedAgents,
    IReadOnlyDictionary<string, object?> Metadata);
