using BrewUp.Shared.Agents;

namespace BrewUp.Warehouse.Facade.Agents;

internal sealed class WarehouseAgentCardProvider : IAgentCardProvider
{
    public AgentCard GetAgentCard()
        => new(
            nameof(WarehouseAgent),
            "Evaluates stock availability, reorder risk, and fulfillment impact.",
            "1.0.0",
            [
                new AgentSkill(
                    "evaluate-stock-impact",
                    "Evaluate current stock, projected remaining quantity, and reorder risk.")
            ],
            [
                new AgentCapability(
                    "evaluate-stock-impact",
                    "Uses only Warehouse MCP tools for availability and reorder thresholds.")
            ]);
}
