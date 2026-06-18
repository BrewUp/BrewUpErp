using System.Text.Json;
using BrewUp.Mother.Facade.Agents;
using BrewUp.Mother.SharedKernel.Chat;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.Agents;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using BrewUp.Shared.ExternalContracts.Warehouse;

namespace BrewUp.Rest.Tests.Chat;

public sealed class MotherCoordinatorTests
{
    private const string MasterDataAgentName = "MasterDataAgent";
    private const string SalesAgentName = "SalesAgent";
    private const string WarehouseAgentName = "WarehouseAgent";
    private const string KnowledgeAgentName = "KnowledgeAgent";

    [Fact]
    public async Task Coordinates_specialized_agents_for_inventory_what_if()
    {
        var mcp = new RecordingMcpToolClient();
        var coordinator = new MotherCoordinator(CreateTestAgents(mcp));

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
            CreateCardProvider(MasterDataAgentName, "Resolves BrewUp beers and product catalog data.", "resolve-beer-catalog"),
            CreateCardProvider(SalesAgentName, "Interprets demand and sales impact.", "interpret-demand-signal"),
            CreateCardProvider(WarehouseAgentName, "Evaluates stock availability and fulfillment impact.", "evaluate-stock-impact"),
            CreateCardProvider(KnowledgeAgentName, "Retrieves documented BrewUp operational knowledge.", "retrieve-business-knowledge")
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

        Assert.Contains(cards, card => card.Name == SalesAgentName
                                       && card.Skills.Any(skill => skill.Name == "interpret-demand-signal"));
        Assert.Contains(cards, card => card.Name == WarehouseAgentName
                                       && card.Capabilities.Any(capability => capability.Name == "evaluate-stock-impact"));
        Assert.Contains(cards, card => card.Name == MasterDataAgentName
                                       && card.Skills.Any(skill => skill.Name == "resolve-beer-catalog"));
        Assert.Contains(cards, card => card.Name == KnowledgeAgentName
                                       && card.Capabilities.Any(capability => capability.Name == "retrieve-business-knowledge"));
    }

    [Fact]
    public void Mother_can_inspect_registered_agent_cards()
    {
        var mcp = new RecordingMcpToolClient();
        var coordinator = new MotherCoordinator(
            CreateTestAgents(mcp),
        [
            CreateCardProvider(MasterDataAgentName, "Resolves BrewUp beers and product catalog data.", "resolve-beer-catalog"),
            CreateCardProvider(SalesAgentName, "Interprets demand and sales impact.", "interpret-demand-signal"),
            CreateCardProvider(WarehouseAgentName, "Evaluates stock availability and fulfillment impact.", "evaluate-stock-impact"),
            CreateCardProvider(KnowledgeAgentName, "Retrieves documented BrewUp operational knowledge.", "retrieve-business-knowledge")
        ]);

        var cards = coordinator.InspectAgentCards();

        Assert.Equal(4, cards.Count);
        Assert.Contains(cards, card => card.Name == KnowledgeAgentName
                                       && card.Skills.Any(skill => skill.Name == "retrieve-business-knowledge"));
    }

    [Fact]
    public async Task Coordinates_knowledge_question_through_a2a_when_enabled()
    {
        var mcp = new RecordingMcpToolClient();
        var a2aClient = new RecordingKnowledgeAgentA2aClient();
        var coordinator = new MotherCoordinator(
            CreateTestAgents(mcp),
            [],
            knowledgeAgentA2aClient: a2aClient,
            a2aOptions: new MotherA2AOptions
            {
                Enabled = true,
                KnowledgeAgentUrl = "http://knowledge-agent"
            });

        var request = new ChatRequest("What is the reorder policy for IPA?", "conversation-a2a");

        Assert.True(coordinator.CanCoordinate(request));

        var response = await coordinator.CoordinateAsync(request, CancellationToken.None);

        Assert.Contains("IPA reorder policy", response.Answer);
        Assert.True(a2aClient.CardDiscovered);
        Assert.Equal("What is the reorder policy for IPA?", a2aClient.LastQuestion);
        Assert.NotEqual(Guid.Empty, a2aClient.LastCorrelationId);
        Assert.DoesNotContain(mcp.Calls, call => call.ServerName == "knowledge");
    }

    private static IAgent[] CreateTestAgents(RecordingMcpToolClient mcp)
        =>
        [
            new TestMasterDataAgent(mcp),
            new TestSalesAgent(mcp),
            new TestWarehouseAgent(mcp),
            new TestKnowledgeAgent(mcp)
        ];

    private static IAgentCardProvider CreateCardProvider(string agentName, string description, string capabilityName)
        => new TestAgentCardProvider(new AgentCard(
            agentName,
            description,
            "1.0.0",
            [new AgentSkill(capabilityName, description)],
            [new AgentCapability(capabilityName, description)]));

    private abstract class TestAgentBase(
        string name,
        string capabilityName,
        string capabilityDescription) : IAgent
    {
        public string Name => name;
        public string SystemPrompt => $"{name} test prompt.";
        public IReadOnlyCollection<AgentCapability> Capabilities { get; } =
        [
            new AgentCapability(capabilityName, capabilityDescription)
        ];

        public bool CanHandle(string capabilityName)
            => Capabilities.Any(capability => capability.Name == capabilityName);

        public abstract Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken);
    }

    private sealed class TestMasterDataAgent(RecordingMcpToolClient mcp) : TestAgentBase(
        MasterDataAgentName,
        "resolve-beer-catalog",
        "Resolves BrewUp beers and product catalog data.")
    {
        public override async Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
        {
            var demandItems = GetInput<IReadOnlyCollection<DemandItem>>(request, "demandItems");
            var resolved = new List<ResolvedBeerDemand>();

            foreach (var demandItem in demandItems)
            {
                var beer = await mcp.CallToolAsync<BeerJson>(
                    "masterData",
                    "masterdata_resolve_beer",
                    new { beerName = demandItem.BeerName },
                    cancellationToken);

                if (beer is not null)
                    resolved.Add(new ResolvedBeerDemand(demandItem, beer));
            }

            return new AgentResponse(
                Name,
                $"Resolved {resolved.Count} demand item(s).",
                new Dictionary<string, object?> { ["resolvedBeerDemand"] = resolved });
        }
    }

    private sealed class TestSalesAgent(RecordingMcpToolClient mcp) : TestAgentBase(
        SalesAgentName,
        "interpret-demand-signal",
        "Interprets demand and sales impact.")
    {
        public override async Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
        {
            var resolved = GetInput<IReadOnlyCollection<ResolvedBeerDemand>>(request, "resolvedBeerDemand");
            var lines = new List<SalesDemandLine>();

            foreach (var item in resolved)
            {
                await mcp.CallToolAsync<object>(
                    "sales",
                    "get_orders_by_beer",
                    new { beerName = item.Beer.BeerName },
                    cancellationToken);

                var unitPrice = item.Beer.Price.Value;
                lines.Add(new SalesDemandLine(
                    item.Beer.BeerId,
                    item.Beer.BeerName,
                    item.Demand.Quantity,
                    item.Demand.UnitOfMeasure,
                    unitPrice,
                    unitPrice * item.Demand.Quantity));
            }

            var signal = new SalesDemandSignal(
                lines,
                lines.Sum(line => line.Quantity),
                lines.Sum(line => line.LineAmount));

            return new AgentResponse(
                Name,
                $"Interpreted demand for {signal.TotalQuantity} item(s).",
                new Dictionary<string, object?> { ["salesDemandSignal"] = signal });
        }
    }

    private sealed class TestWarehouseAgent(RecordingMcpToolClient mcp) : TestAgentBase(
        WarehouseAgentName,
        "evaluate-stock-impact",
        "Evaluates stock availability and fulfillment impact.")
    {
        public override async Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
        {
            var salesSignal = GetInput<SalesDemandSignal>(request, "salesDemandSignal");
            var lines = new List<WarehouseImpactLine>();

            foreach (var line in salesSignal.Lines)
            {
                var availability = await mcp.CallToolAsync<AvailabilityWithThresholdJson>(
                    "warehouse",
                    "get_beer_availability",
                    new { beerId = line.BeerId },
                    cancellationToken);

                var thresholdJson = await mcp.CallToolAsync<JsonElement>(
                    "warehouse",
                    "get_reorder_thresholds",
                    new { beerId = line.BeerId },
                    cancellationToken);

                var available = availability?.Quantity ?? 0;
                var reorderThreshold = ExtractThreshold(thresholdJson, availability?.ReorderThreshold ?? 0);
                var remaining = available - line.Quantity;

                lines.Add(new WarehouseImpactLine(
                    line.BeerId,
                    line.BeerName,
                    line.Quantity,
                    available,
                    remaining,
                    reorderThreshold,
                    remaining < 0,
                    remaining >= 0 && remaining <= reorderThreshold,
                    availability?.UnitOfMeasure ?? line.UnitOfMeasure));
            }

            var impact = new WarehouseImpact(
                lines,
                lines.Any(line => line.StockRisk),
                lines.Any(line => line.ReorderRisk));

            return new AgentResponse(
                Name,
                "Evaluated warehouse impact.",
                new Dictionary<string, object?> { ["warehouseImpact"] = impact });
        }
    }

    private sealed class TestKnowledgeAgent(RecordingMcpToolClient mcp) : TestAgentBase(
        KnowledgeAgentName,
        "retrieve-business-knowledge",
        "Retrieves documented BrewUp operational knowledge.")
    {
        public override async Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
        {
            var resolved = GetInput<IReadOnlyCollection<ResolvedBeerDemand>>(request, "resolvedBeerDemand");
            var query = $"reorder policy {string.Join(' ', resolved.Select(item => item.Beer.BeerStyle))}";

            var searchResult = await mcp.CallToolAsync<SearchKnowledgeResult>(
                "knowledge",
                "search_knowledge_base",
                new { query },
                cancellationToken);

            var findings = searchResult?.Items
                .Select(result => new KnowledgeFinding(
                    result.Title,
                    result.Scope,
                    result.Content,
                    result.Score))
                .ToArray() ?? [];

            return new AgentResponse(
                Name,
                $"Retrieved {findings.Length} knowledge finding(s).",
                new Dictionary<string, object?> { ["knowledgeResult"] = new KnowledgeResult(findings) });
        }
    }

    private sealed class TestAgentCardProvider(AgentCard card) : IAgentCardProvider
    {
        public AgentCard GetAgentCard() => card;
    }

    private sealed class RecordingKnowledgeAgentA2aClient : IKnowledgeAgentA2AClient
    {
        public bool CardDiscovered { get; private set; }
        public string? LastQuestion { get; private set; }
        public Guid LastCorrelationId { get; private set; }

        public Task<AgentCard> GetAgentCardAsync(CancellationToken cancellationToken)
        {
            CardDiscovered = true;

            return Task.FromResult(new AgentCard(
                "BrewUp Knowledge Agent",
                "Provides access to documented BrewUp business knowledge, operational procedures, company policies, brewery processes, and business rules.",
                "1.0.0",
                [new AgentSkill("search_knowledge", "Search BrewUp documented knowledge.")],
                [new AgentCapability("knowledge retrieval", "Retrieves documented BrewUp business knowledge.")]));
        }

        public Task<KnowledgeResult> SubmitKnowledgeTaskAsync(
            string question,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            LastQuestion = question;
            LastCorrelationId = correlationId;

            return Task.FromResult(new KnowledgeResult(
            [
                new KnowledgeFinding(
                    "IPA reorder policy",
                    "Warehouse",
                    "IPA reorder policy: review replenishment when projected stock reaches the threshold.",
                    0.91)
            ]));
        }
    }

    private static T GetInput<T>(AgentRequest request, string key)
    {
        Assert.True(request.Inputs.TryGetValue(key, out var value), $"Missing input '{key}'.");
        return Assert.IsAssignableFrom<T>(value);
    }

    private static decimal ExtractThreshold(JsonElement thresholdJson, decimal fallback)
    {
        if (thresholdJson.ValueKind == JsonValueKind.Object
            && thresholdJson.TryGetProperty("thresholdQuantity", out var thresholdQuantity)
            && thresholdQuantity.ValueKind == JsonValueKind.Object
            && thresholdQuantity.TryGetProperty("value", out var value)
            && value.TryGetDecimal(out var threshold))
        {
            return threshold;
        }

        return fallback;
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
