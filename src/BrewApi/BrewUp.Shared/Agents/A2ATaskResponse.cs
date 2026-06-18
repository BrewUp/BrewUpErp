namespace BrewUp.Shared.Agents;

public sealed record A2ATaskResponse(
    string TaskId,
    string AgentName,
    bool IsSuccessful,
    string Summary,
    KnowledgeResult KnowledgeResult,
    Guid CorrelationId);
