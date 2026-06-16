namespace BrewUp.Mother.Facade.Agents;

public sealed class SalesAgentCardProvider : IAgentCardProvider
{
    public AgentCard GetAgentCard()
        => new(
            nameof(SalesAgent),
            "Interprets customer demand and sales order impact from the Sales bounded context.",
            "1.0.0",
            [
                new AgentSkill(
                    "interpret-demand-signal",
                    "Interpret resolved demand as sales quantity and estimated value.")
            ],
            [
                new AgentCapability(
                    "interpret-demand-signal",
                    "Uses only Sales MCP tools for sales demand context.")
            ]);
}
