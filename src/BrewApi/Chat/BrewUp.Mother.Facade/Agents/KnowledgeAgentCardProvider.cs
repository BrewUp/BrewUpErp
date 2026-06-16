namespace BrewUp.Mother.Facade.Agents;

public sealed class KnowledgeAgentCardProvider : IAgentCardProvider
{
    public AgentCard GetAgentCard()
        => new(
            nameof(KnowledgeAgent),
            "Retrieves documented BrewUp policies, procedures, operational rules, and business knowledge.",
            "1.0.0",
            [
                new AgentSkill(
                    "retrieve-business-knowledge",
                    "Retrieve documented policies and operational rules relevant to a scenario.")
            ],
            [
                new AgentCapability(
                    "retrieve-business-knowledge",
                    "Uses only Knowledge MCP tools, including search_knowledge_base.")
            ]);
}
