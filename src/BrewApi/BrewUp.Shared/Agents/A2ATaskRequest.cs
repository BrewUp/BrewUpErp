namespace BrewUp.Shared.Agents;

public sealed record A2ATaskRequest(
    string TaskId,
    string Message,
    Guid CorrelationId,
    IReadOnlyDictionary<string, object?> Metadata);