using System.Text.Json;
using BrewUp.Mother.Facade.Agents;
using BrewUp.Mother.McpClients;
using BrewUp.Mother.SharedKernel.Chat;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using BrewUp.Shared.ExternalContracts.Warehouse;

namespace BrewUp.Rest.Tests.Chat;

public sealed class MotherCoordinatorTests
{
    [Fact]
    public async Task Coordinates_specialized_agents_for_inventory_what_if()
    {
        var mcp = new RecordingMcpToolClient();
        var coordinator = new MotherCoordinator(
        [
            new MasterDataAgent(mcp),
            new SalesAgent(mcp),
            new WarehouseAgent(mcp),
            new KnowledgeAgent(mcp)
        ]);

        var response = await coordinator.CoordinateAsync(
            new ChatRequest("What if someone orders 100 bottles of IPA?", "conversation-1"),
            CancellationToken.None);

        Assert.Contains("Scenario interpreted as demand for 100 bottle", response.Answer);
        Assert.Contains("IPA", response.Answer);
        Assert.Contains("Stock risk", response.Answer);
        Assert.Contains("IPA reorder policy", response.Answer);

        Assert.Contains(mcp.Calls, call => call.ServerName == "masterData" && call.ToolName == "masterdata_resolve_beer");
        Assert.Contains(mcp.Calls, call => call.ServerName == "sales" && call.ToolName == "get_orders_by_beer");
        Assert.Contains(mcp.Calls, call => call.ServerName == "warehouse" && call.ToolName == "get_beer_availability");
        Assert.Contains(mcp.Calls, call => call.ServerName == "warehouse" && call.ToolName == "get_reorder_thresholds");
        Assert.Contains(mcp.Calls, call => call.ServerName == "knowledge" && call.ToolName == "search_knowledge_base");

        Assert.DoesNotContain(mcp.Calls, call => call.ServerName == "masterData" && call.ToolName.StartsWith("get_orders", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(mcp.Calls, call => call.ServerName == "warehouse" && call.ToolName == "search_knowledge_base");
        Assert.DoesNotContain(mcp.Calls, call => call.ServerName == "knowledge" && call.ToolName != "search_knowledge_base");
    }

    [Fact]
    public void Agent_request_carries_future_agent_context()
    {
        var context = new AgentContext(
            "conversation-1",
            "Mother",
            ["MasterDataAgent", "WarehouseAgent"],
            new Dictionary<string, object?> { ["businessDate"] = "2026-06-16" });

        var request = new AgentRequest(
            "evaluate-stock-impact",
            "What if someone orders 100 bottles of IPA?",
            new Dictionary<string, object?>(),
            Guid.CreateVersion7(),
            context);

        Assert.Equal("conversation-1", request.Context.ConversationId);
        Assert.Contains("WarehouseAgent", request.Context.InvokedAgents);
    }

    [Fact]
    public void Agent_card_providers_describe_specialized_agents()
    {
        IAgentCardProvider[] providers =
        [
            new MasterDataAgentCardProvider(),
            new SalesAgentCardProvider(),
            new WarehouseAgentCardProvider(),
            new KnowledgeAgentCardProvider()
        ];

        var cards = providers.Select(provider => provider.GetAgentCard()).ToArray();

        Assert.All(cards, card =>
        {
            Assert.False(string.IsNullOrWhiteSpace(card.Name));
            Assert.False(string.IsNullOrWhiteSpace(card.Description));
            Assert.False(string.IsNullOrWhiteSpace(card.Version));
            Assert.NotEmpty(card.Skills);
            Assert.NotEmpty(card.Capabilities);
        });

        Assert.Contains(cards, card => card.Name == nameof(SalesAgent)
                                       && card.Skills.Any(skill => skill.Name == "interpret-demand-signal"));
        Assert.Contains(cards, card => card.Name == nameof(WarehouseAgent)
                                       && card.Capabilities.Any(capability => capability.Name == "evaluate-stock-impact"));
        Assert.Contains(cards, card => card.Name == nameof(MasterDataAgent)
                                       && card.Skills.Any(skill => skill.Name == "resolve-beer-catalog"));
        Assert.Contains(cards, card => card.Name == nameof(KnowledgeAgent)
                                       && card.Capabilities.Any(capability => capability.Name == "retrieve-business-knowledge"));
    }

    [Fact]
    public void Mother_can_inspect_registered_agent_cards()
    {
        var mcp = new RecordingMcpToolClient();
        var coordinator = new MotherCoordinator(
        [
            new MasterDataAgent(mcp),
            new SalesAgent(mcp),
            new WarehouseAgent(mcp),
            new KnowledgeAgent(mcp)
        ],
        [
            new MasterDataAgentCardProvider(),
            new SalesAgentCardProvider(),
            new WarehouseAgentCardProvider(),
            new KnowledgeAgentCardProvider()
        ]);

        var cards = coordinator.InspectAgentCards();

        Assert.Equal(4, cards.Count);
        Assert.Contains(cards, card => card.Name == nameof(KnowledgeAgent)
                                       && card.Skills.Any(skill => skill.Name == "retrieve-business-knowledge"));
    }

    private sealed class RecordingMcpToolClient : IMcpToolClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly List<McpCall> _calls = [];

        public IReadOnlyCollection<McpCall> Calls => _calls.AsReadOnly();

        public Task<TResponse?> CallToolAsync<TResponse>(
            string serverName,
            string toolName,
            object arguments,
            CancellationToken cancellationToken)
        {
            _calls.Add(new McpCall(serverName, toolName));

            var argumentJson = JsonSerializer.SerializeToElement(arguments, JsonOptions);

            object? response = (serverName.ToLowerInvariant(), toolName) switch
            {
                ("masterdata", "masterdata_resolve_beer") => ResolveBeer(argumentJson.GetProperty("beerName").GetString()!),
                ("sales", "get_orders_by_beer") => Array.Empty<object>(),
                ("warehouse", "get_beer_availability") => GetAvailability(argumentJson.GetProperty("beerId").GetString()!),
                ("warehouse", "get_reorder_thresholds") => GetThreshold(argumentJson.GetProperty("beerId").GetString()!),
                ("knowledge", "search_knowledge_base") => GetKnowledgePolicy(),
                _ => default(TResponse)
            };

            return Task.FromResult((TResponse?)response);
        }

        private static BeerJson ResolveBeer(string beerName)
            => beerName.Contains("weiss", StringComparison.OrdinalIgnoreCase)
                ? new BeerJson
                {
                    BeerId = "beer-weiss",
                    BeerName = "Muflone Weiss",
                    BeerStyle = "Weiss",
                    Price = new Price(4.2m, "EUR")
                }
                : new BeerJson
                {
                    BeerId = "beer-ipa",
                    BeerName = "BrewUp IPA",
                    BeerStyle = "IPA",
                    Price = new Price(5m, "EUR")
                };

        private static AvailabilityWithThresholdJson GetAvailability(string beerId)
            => beerId == "beer-weiss"
                ? new AvailabilityWithThresholdJson
                {
                    Id = "availability-weiss",
                    WarehouseId = "warehouse-main",
                    BeerId = beerId,
                    Quantity = 120,
                    ReorderThreshold = 40,
                    UnitOfMeasure = "Bottle"
                }
                : new AvailabilityWithThresholdJson
                {
                    Id = "availability-ipa",
                    WarehouseId = "warehouse-main",
                    BeerId = beerId,
                    Quantity = 70,
                    ReorderThreshold = 30,
                    UnitOfMeasure = "Bottle"
                };

        private static JsonElement GetThreshold(string beerId)
            => JsonSerializer.SerializeToElement(
                new
                {
                    beerId,
                    thresholdQuantity = new
                    {
                        value = 30,
                        unitOfMeasure = "Bottle"
                    }
                },
                JsonOptions);

        private static SearchKnowledgeResult GetKnowledgePolicy()
            => new(
            [
                new KnowledgeSearchResultItem(
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7(),
                    1,
                    "IPA reorder policy",
                    "Warehouse",
                    ["ipa", "reorder"],
                    "IPA reorder policy: review replenishment when projected stock reaches the threshold.",
                    0.91,
                    12)
            ]);
    }

    private sealed record McpCall(string ServerName, string ToolName);
}
