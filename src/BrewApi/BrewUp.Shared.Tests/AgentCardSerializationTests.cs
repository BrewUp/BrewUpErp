using System.Text.Json;
using BrewUp.Shared.Agents;

namespace BrewUp.Shared.Tests;

public sealed class AgentCardSerializationTests
{
    [Fact]
    public void Agent_card_round_trips_with_examples()
    {
        var card = new AgentCard(
            "BrewUp Knowledge Agent",
            "Provides access to documented BrewUp business knowledge.",
            "1.0.0",
            [new AgentSkill("search_knowledge", "Search documented knowledge.")],
            [new AgentCapability("knowledge retrieval", "Retrieves documented knowledge.")],
            ["What is the reorder policy for IPA?"]);

        var json = JsonSerializer.Serialize(card, JsonSerializerOptions.Web);

        var deserialized = JsonSerializer.Deserialize<AgentCard>(json, JsonSerializerOptions.Web);

        Assert.NotNull(deserialized);
        Assert.Equal(card.Name, deserialized.Name);
        Assert.Contains("What is the reorder policy for IPA?", deserialized.Examples);
    }
}
