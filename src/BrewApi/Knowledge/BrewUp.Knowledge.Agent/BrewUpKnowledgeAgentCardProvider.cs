using BrewUp.Shared.Agents;

namespace BrewUp.Knowledge.Agent;

public sealed class BrewUpKnowledgeAgentCardProvider : IAgentCardProvider
{
    public AgentCard GetAgentCard()
        => new(
            "BrewUp Knowledge Agent",
            "Provides access to documented BrewUp business knowledge, operational procedures, company policies, brewery processes, and business rules.",
            "1.0.0",
            [
                new AgentSkill(
                    "search_knowledge",
                    "Search documented BrewUp business knowledge, operational procedures, company policies, brewery processes, and business rules.")
            ],
            [
                new AgentCapability("knowledge retrieval", "Retrieves documented BrewUp business knowledge."),
                new AgentCapability("documentation lookup", "Looks up BrewUp documentation."),
                new AgentCapability("policy lookup", "Looks up company and operational policies."),
                new AgentCapability("procedure lookup", "Looks up documented operational procedures.")
            ],
            [
                "What is the reorder policy for IPA?",
                "How does inventory management work?",
                "How is beer produced?",
                "What are the quality standards?"
            ]);
}
