using BrewUp.Mother.Agents;
using BrewUp.Mother.Clients;
using BrewUp.Mother.CustomTypes;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.Messages.Events.Sagas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muflone;
using Muflone.Transport.InMemory;
using NSubstitute;

namespace BrewUp.Mother.Tests;

/// <summary>
/// Proves that <see cref="InventoryRiskAgent"/> actually receives a
/// <see cref="SalesOrderConfirmed"/> integration event published on the
/// InMemory bus and executes its full processing pipeline.
/// </summary>
public sealed class InventoryRiskAgentTest : IAsyncLifetime
{
    // ── shared mocks ───────────────────────────────────────────────────────────
    private readonly IMcpToolClient _mcpToolClient = Substitute.For<IMcpToolClient>();
    private readonly IRecommendationWriter _recommendationWriter = Substitute.For<IRecommendationWriter>();

    // ── test data ──────────────────────────────────────────────────────────────
    private readonly string _salesOrderId = Guid.CreateVersion7().ToString();
    private const string BeerId = "beer-golden-ale-001";
    private const string BeerName = "Golden Ale";

    // The order requires 10 units; availability is 12 with a reorder threshold of 5.
    // → residual = 12 – 10 = 2  <  threshold 5  → StockRiskDetected must be written.
    private const decimal RequiredQty = 10m;
    private const decimal AvailableQty = 12m;
    private const decimal ReorderThreshold = 5m;

    private IHost _host = null!;

    // ── IAsyncLifetime ─────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        // --- stub: get_sales_order_details → returns an order with one beer row ---
        _mcpToolClient
            .CallToolAsync<SalesOrderJson>(
                Arg.Is("sales"),
                Arg.Is("get_sales_order_details"),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(new SalesOrderJson
            {
                Id = _salesOrderId,
                OrderNumber = "20260101-test",
                OrderDate = DateTime.UtcNow,
                CustomerId = Guid.CreateVersion7().ToString(),
                CustomerName = "Test Brewery",
                DeliveryDate = DateTime.UtcNow.AddDays(7),
                Status = "Confirmed",
                Rows =
                [
                    new SalesOrderRowJson
                    {
                        BeerId = BeerId,
                        BeerName = BeerName,
                        Quantity = new Quantity(RequiredQty, "pcs"),
                        Price = new Price(5.00m, "EUR")
                    }
                ]
            });

        // --- stub: get_beer_availability → stock that will breach the threshold ---
        _mcpToolClient
            .CallToolAsync<AvailabilityWithThresholdJson>(
                Arg.Is("warehouse"),
                Arg.Is("get_beer_availability"),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(new AvailabilityWithThresholdJson
            {
                Id = Guid.CreateVersion7().ToString(),
                WarehouseId = Guid.CreateVersion7().ToString(),
                BeerId = BeerId,
                Quantity = AvailableQty,
                ReorderThreshold = ReorderThreshold,
                UnitOfMeasure = "pcs"
            });

        // --- stub: WriteAsync → no-op (we only want to verify it is called) ---
        _recommendationWriter
            .WriteAsync(Arg.Any<StockRiskDetected>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // --- build and start the host -----------------------------------------
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // InMemory broker: events published in-process, no RabbitMq needed.
                services.AddMufloneTransportInMemory();

                // Registers InventoryRiskAgent as an IntegrationEventHandler.
                // AddMother also registers IMcpToolClient and IRecommendationWriter,
                // so we override them AFTER so the last registration wins.
                services.AddMother();

                // Override with stubs AFTER AddMother (last registration wins in .NET DI).
                services.AddScoped<IMcpToolClient>(_ => _mcpToolClient);
                services.AddScoped<IRecommendationWriter>(_ => _recommendationWriter);

                // Start the Worker BackgroundService (as in production).
                services.AddHostedService<Worker>();
            })
            .Build();

        await _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    // ── tests ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task InventoryRiskAgent_Receives_SalesOrderConfirmed_And_Writes_StockRiskDetected()
    {
        // Arrange ─────────────────────────────────────────────────────────────
        var eventBus = _host.Services.GetRequiredService<IEventBus>();

        var @event = new SalesOrderConfirmed(
            aggregateId: new IntegrationId(_salesOrderId),
            correlationId: Guid.CreateVersion7(),
            salesOrderNumber: "20260101-test",
            salesOrderDate: DateTime.UtcNow,
            customerId: Guid.CreateVersion7().ToString(),
            salesOrderDeliveryDate: DateTime.UtcNow.AddDays(7),
            rows: []);   // rows are irrelevant here; the agent re-fetches from MCP

        // Act ─────────────────────────────────────────────────────────────────
        await eventBus.PublishAsync(@event, CancellationToken.None);

        // Give the in-process handler time to complete.
        await Task.Delay(500);

        // Assert ──────────────────────────────────────────────────────────────

        // 1. The agent called the Sales MCP to fetch the order.
        await _mcpToolClient.Received(1)
            .CallToolAsync<SalesOrderJson>(
                "sales",
                "get_sales_order_details",
                Arg.Any<object>(),
                Arg.Any<CancellationToken>());

        // 2. The agent called the Warehouse MCP to check availability for the beer.
        await _mcpToolClient.Received(1)
            .CallToolAsync<AvailabilityWithThresholdJson>(
                "warehouse",
                "get_beer_availability",
                Arg.Any<object>(),
                Arg.Any<CancellationToken>());

        // 3. Because the residual stock (12 − 10 = 2) is below the threshold (5),
        //    the agent must have written a StockRiskDetected recommendation.
        await _recommendationWriter.Received(1)
            .WriteAsync(
                Arg.Is<StockRiskDetected>(r =>
                    r.BeerId == BeerId &&
                    r.RequiredQuantity == RequiredQty &&
                    r.AvailableQuantity == AvailableQty &&
                    r.ReorderThreshold == ReorderThreshold),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InventoryRiskAgent_DoesNot_WriteRisk_When_Stock_IsAboveThreshold()
    {
        // Arrange ─────────────────────────────────────────────────────────────
        // Override availability so stock stays above threshold after the order.
        // residual = 100 – 10 = 90  ≥  threshold 5  → no risk
        _mcpToolClient
            .CallToolAsync<AvailabilityWithThresholdJson>(
                Arg.Is("warehouse"),
                Arg.Is("get_beer_availability"),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(new AvailabilityWithThresholdJson
            {
                Id = Guid.CreateVersion7().ToString(),
                WarehouseId = Guid.CreateVersion7().ToString(),
                BeerId = BeerId,
                Quantity = 100m,
                ReorderThreshold = ReorderThreshold,
                UnitOfMeasure = "pcs"
            });

        var eventBus = _host.Services.GetRequiredService<IEventBus>();

        var @event = new SalesOrderConfirmed(
            aggregateId: new IntegrationId(_salesOrderId),
            correlationId: Guid.CreateVersion7(),
            salesOrderNumber: "20260101-no-risk",
            salesOrderDate: DateTime.UtcNow,
            customerId: Guid.CreateVersion7().ToString(),
            salesOrderDeliveryDate: DateTime.UtcNow.AddDays(7),
            rows: []);

        // Act ─────────────────────────────────────────────────────────────────
        await eventBus.PublishAsync(@event, CancellationToken.None);
        await Task.Delay(500);

        // Assert ──────────────────────────────────────────────────────────────
        // The writer must NOT have been called.
        await _recommendationWriter.DidNotReceive()
            .WriteAsync(Arg.Any<StockRiskDetected>(), Arg.Any<CancellationToken>());
    }
}

