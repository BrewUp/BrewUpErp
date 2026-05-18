using BrewUp.Mother.Clients;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using BrewUp.Shared.Messages.Events.Sagas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Muflone;
using Muflone.Transport.InMemory;
using NSubstitute;

namespace BrewUp.Mother.Tests;

[Collection(MotherTestCollection.Name)]
public class MotherStartupTest : IAsyncLifetime
{
    private IHost _host = null!;

    public async Task InitializeAsync()
    {
        var mcpToolClient = Substitute.For<IMcpToolClient>();
        mcpToolClient
            .CallToolAsync<SalesOrderJson>(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns((SalesOrderJson?)null);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMufloneTransportInMemory();
                services.AddMother();
                // Replace (not add) so that the real McpToolClient — which needs
                // IHttpClientFactory — is removed. Otherwise ValidateOnBuild fails.
                services.Replace(ServiceDescriptor.Scoped<IMcpToolClient>(_ => mcpToolClient));
                services.AddHostedService<Worker>();
            })
            .Build();

        await _host.StartAsync();
    }

    [Fact]
    public async Task Can_Mother_Starts()
    {
        var integrationId = new IntegrationId(Guid.CreateVersion7().ToString());
        var salesOrderNumber =
            $"{DateTime.Now.Year:0000}{DateTime.Now.Month:00}{DateTime.Now.Day:00}-{Guid.NewGuid().ToString()[..8]}";
        var customerId = Guid.CreateVersion7().ToString();

        var eventBus = _host.Services.GetRequiredService<IEventBus>();

        var integrationEvent = new SalesOrderConfirmed(
            integrationId,
            Guid.CreateVersion7(),
            salesOrderNumber,
            DateTime.Now,
            customerId,
            DateTime.Now.AddDays(7),
            new List<SalesOrderRowJson>());

        await eventBus.PublishAsync(integrationEvent, CancellationToken.None);

        await Task.Delay(200);
    }
    
    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }
}