using System.ComponentModel;
using BrewUp.Mother.Facade.Mcp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using KnowledgeTools = BrewUp.Knowledge.McpServer.Tools.KnowledgeTools;
using MasterDataTools = BrewUp.MasterData.McpServer.Tools.MasterDataTools;
using SalesTools = BrewUp.Sales.McpServer.Tools.SalesTools;
using WarehouseTools = BrewUp.Warehouse.McpServer.Tools.WarehouseTools;

namespace BrewUp.Mcp.IntegrationTests;

public sealed class McpToolsProviderTests
{
    [Fact]
    public Task MasterData_exposes_stable_tool_contracts() =>
        AssertToolCatalogAsync<MasterDataTools>(
            "get_catalog_beers",
            "get_beer_details",
            "resolve-beer-catalog",
            "get_active_customers",
            "get_customer_info",
            "get_active_suppliers",
            "get_supplier_info",
            "get_active_warehouses");

    [Fact]
    public Task Sales_exposes_stable_tool_contracts() =>
        AssertToolCatalogAsync<SalesTools>(
            "get_open_sales_orders",
            "get_sales_order_details",
            "get_orders_by_customer",
            "get_orders_by_beer",
            "get_late_sales_orders",
            "get_customer_total_purchased",
            "get_sales_order_total_quantities");

    [Fact]
    public Task Warehouse_exposes_stable_tool_contracts() =>
        AssertToolCatalogAsync<WarehouseTools>(
            "get_beer_availability",
            "get_reorder_thresholds");

    [Fact]
    public Task Knowledge_exposes_stable_tool_contracts() =>
        AssertToolCatalogAsync<KnowledgeTools>("search_knowledge_base");

    [Fact]
    public async Task Aggregates_healthy_servers_and_isolates_unavailable_server()
    {
        await using var masterDataServer = await CreateServerAsync<MasterDataTestTools>();
        await using var salesServer = await CreateServerAsync<SalesTestTools>();
        using var handler = new RoutingHandler(new Dictionary<string, HttpMessageHandler>
        {
            ["masterdata"] = masterDataServer.GetTestServer().CreateHandler(),
            ["sales"] = salesServer.GetTestServer().CreateHandler()
        });
        var factory = new TestHttpClientFactory(handler);
        var timeProvider = new TestTimeProvider();
        await using var provider = new McpToolsProvider(
            new BrewUp.Mother.Facade.Mcp.McpServerOptions
            {
                MasterDataUrl = "http://masterdata/mcp",
                SalesUrl = "http://sales/mcp",
                WarehouseUrl = "http://unavailable/mcp"
            },
            factory,
            NullLoggerFactory.Instance,
            timeProvider);

        var tools = await provider.GetToolsAsync(CancellationToken.None);

        Assert.Contains(tools, tool => tool.Name == "get_catalog_beers");
        Assert.Contains(tools, tool => tool.Name == "get_open_sales_orders");
        Assert.Equal(2, tools.Count);
    }

    [Fact]
    public async Task Caches_empty_catalog_and_refreshes_only_after_interval()
    {
        await using var server = await CreateServerAsync<EmptyTestTools>();
        using var handler = new RoutingHandler(new Dictionary<string, HttpMessageHandler>
        {
            ["empty"] = server.GetTestServer().CreateHandler()
        });
        var factory = new TestHttpClientFactory(handler);
        var timeProvider = new TestTimeProvider();
        await using var provider = new McpToolsProvider(
            new BrewUp.Mother.Facade.Mcp.McpServerOptions { MasterDataUrl = "http://empty/mcp" },
            factory,
            NullLoggerFactory.Instance,
            timeProvider);

        Assert.Empty(await provider.GetToolsAsync(CancellationToken.None));
        var requestsAfterFirstDiscovery = handler.RequestCount;

        Assert.Empty(await provider.GetToolsAsync(CancellationToken.None));
        Assert.Equal(requestsAfterFirstDiscovery, handler.RequestCount);

        timeProvider.Advance(TimeSpan.FromMinutes(6));
        Assert.Empty(await provider.GetToolsAsync(CancellationToken.None));
        Assert.True(handler.RequestCount > requestsAfterFirstDiscovery);
    }

    private static async Task<WebApplication> CreateServerAsync<TTools>()
        where TTools : class
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddMcpServer()
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools<TTools>();

        var app = builder.Build();
        app.MapMcp("/mcp");
        await app.StartAsync();
        return app;
    }

    private static async Task AssertToolCatalogAsync<TTools>(params string[] expectedTools)
        where TTools : class
    {
        await using var server = await CreateServerAsync<TTools>();
        using var httpClient = server.GetTestClient();
        await using var client = await McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri("http://localhost/mcp"),
                    Name = typeof(TTools).Name
                },
                httpClient,
                NullLoggerFactory.Instance),
            new McpClientOptions
            {
                ClientInfo = new Implementation { Name = "BrewUp.ContractTests", Version = "1.0.0" }
            },
            NullLoggerFactory.Instance);

        var tools = await client.ListToolsAsync();

        Assert.Equal(
            expectedTools.Order(StringComparer.Ordinal),
            tools.Select(tool => tool.Name).Order(StringComparer.Ordinal));
    }

    [McpServerToolType]
    private sealed class MasterDataTestTools
    {
        [McpServerTool(Name = "get_catalog_beers")]
        [Description("Gets the beer catalog.")]
        public static string GetCatalogBeers() => "IPA";
    }

    [McpServerToolType]
    private sealed class SalesTestTools
    {
        [McpServerTool(Name = "get_open_sales_orders")]
        [Description("Gets open sales orders.")]
        public static string GetOpenSalesOrders() => "SO-1000";
    }

    [McpServerToolType]
    private sealed class EmptyTestTools;

    private sealed class TestHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow = new(2026, 8, 26, 8, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow += duration;
    }

    private sealed class RoutingHandler : HttpMessageHandler
    {
        private readonly IReadOnlyDictionary<string, HttpMessageInvoker> _routes;

        public RoutingHandler(IReadOnlyDictionary<string, HttpMessageHandler> routes)
        {
            _routes = routes.ToDictionary(
                route => route.Key,
                route => new HttpMessageInvoker(route.Value),
                StringComparer.OrdinalIgnoreCase);
        }

        public int RequestCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;

            var host = request.RequestUri?.Host
                       ?? throw new InvalidOperationException("MCP request has no host.");
            if (!_routes.TryGetValue(host, out var route))
                throw new HttpRequestException($"Test MCP endpoint '{host}' is unavailable.");

            return await route.SendAsync(request, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var route in _routes.Values)
                    route.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
