using BrewUp.Shared.Agents;

namespace BrewUp.Mother.Facade.Agents;

public interface IKnowledgeAgentA2AClient
{
    Task<AgentCard> GetAgentCardAsync(CancellationToken cancellationToken);

    Task<KnowledgeResult> SubmitKnowledgeTaskAsync(
        string question,
        Guid correlationId,
        string? conversationId,
        CancellationToken cancellationToken);
}
