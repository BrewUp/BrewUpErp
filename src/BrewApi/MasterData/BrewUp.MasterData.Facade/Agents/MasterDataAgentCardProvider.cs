using BrewUp.Shared.Agents;

namespace BrewUp.MasterData.Facade.Agents;

internal sealed class MasterDataAgentCardProvider : IAgentCardProvider
{
    public AgentCard GetAgentCard()
        => new(
            nameof(MasterDataAgent),
            "Resolves BrewUp products, beers, styles, customers, and suppliers from MasterData.",
            "1.0.0",
            [
                new AgentSkill(
                    "resolve-beer-catalog",
                    "Resolve requested beer names into MasterData catalog records.")
            ],
            [
                new AgentCapability(
                    "resolve-beer-catalog",
                    "Uses only MasterData MCP tools to resolve beer catalog data.")
            ]);
}
