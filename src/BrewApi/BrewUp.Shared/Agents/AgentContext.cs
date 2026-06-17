namespace BrewUp.Shared.Agents;

public sealed record AgentContext(
    string ConversationId,
    string RequestedBy,
    IReadOnlyCollection<string> InvokedAgents,
    IReadOnlyDictionary<string, object?> Metadata);