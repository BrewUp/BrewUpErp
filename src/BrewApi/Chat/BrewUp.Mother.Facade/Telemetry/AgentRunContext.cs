namespace BrewUp.Mother.Facade.Telemetry;

public sealed class AgentRunContext(Guid runId, string? conversationId)
{
    public Guid RunId { get; } = runId;
    public string? ConversationId { get; } = conversationId;
    public string Outcome { get; private set; } = "completed";

    public void SetOutcome(string outcome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);
        Outcome = outcome;
    }
}