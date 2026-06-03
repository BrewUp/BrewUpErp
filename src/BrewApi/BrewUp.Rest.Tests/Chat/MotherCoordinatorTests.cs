using System.Text.Json;
using BrewUp.Mother.Facade.Agents;
using BrewUp.Mother.McpClients;
using BrewUp.Mother.SharedKernel.Chat;
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
            new SalesAgent(),
            new WarehouseAgent(mcp)
        ]);

        var response = await coordinator.CoordinateAsync(
            new ChatRequest(
                "What if someone orders 50 bottles of IPA and 100 bottles of Weiss?",
                "conversation-1"),
            CancellationToken.None);

        Assert.Contains("Scenario interpreted as demand for 150 bottle", response.Answer);
        Assert.Contains("IPA", response.Answer);
        Assert.Contains("Weiss", response.Answer);
        Assert.Contains("Reorder risk", response.Answer);

        Assert.Contains(mcp.Calls, call => call.ServerName == "masterData" && call.ToolName == "masterdata_resolve_beer");
        Assert.Contains(mcp.Calls, call => call.ServerName == "warehouse" && call.ToolName == "get_beer_availability");
        Assert.Contains(mcp.Calls, call => call.ServerName == "warehouse" && call.ToolName == "get_reorder_thresholds");
        Assert.DoesNotContain(mcp.Calls, call => call.ServerName.Equals("sales", StringComparison.OrdinalIgnoreCase));
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
                ("warehouse", "get_beer_availability") => GetAvailability(argumentJson.GetProperty("beerId").GetString()!),
                ("warehouse", "get_reorder_thresholds") => GetThreshold(argumentJson.GetProperty("beerId").GetString()!),
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
    }

    private sealed record McpCall(string ServerName, string ToolName);
}
