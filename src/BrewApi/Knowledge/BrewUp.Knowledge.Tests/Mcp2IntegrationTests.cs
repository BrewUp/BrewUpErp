using System.Diagnostics;
using BrewUp.Knowledge.Agent.Telemetry;
using BrewUp.Knowledge.Agent.Tools;
using BrewUp.Knowledge.McpServer;
using BrewUp.Knowledge.McpServer.Tools;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared;
using BrewUp.Shared.Agents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BrewUp.Knowledge.Tests;

public sealed class Mcp2IntegrationTests
{
    [Fact]
    public async Task Stateless_server_supports_discovery_listing_and_invocation_without_a_session()
    {
        await using var server = await CreateServerAsync();
        var recordingHandler = new RecordingHandler(server.GetTestServer().CreateHandler());
        using var httpClient = new HttpClient(recordingHandler);
        await using var client = await McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri("http://localhost/mcp"),
                    Name = "Knowledge"
                },
                httpClient,
                NullLoggerFactory.Instance),
            new McpClientOptions
            {
                ClientInfo = new Implementation { Name = "BrewUp.Tests", Version = "1.0.0" }
            },
            NullLoggerFactory.Instance);

        var tools = await client.ListToolsAsync();
        var tool = Assert.Single(tools);
        Assert.Equal("search_knowledge_base", tool.Name);

        var result = await client.CallToolAsync(
            tool.Name,
            new Dictionary<string, object?>
            {
                ["query"] = "refund policy",
                ["scope"] = "Sales",
                ["topK"] = 3
            });

        Assert.NotEqual(true, result.IsError);
        Assert.NotEmpty(result.Content);
        Assert.False(recordingHandler.SawSessionHeader);
        Assert.Contains("server/discover", recordingHandler.Methods);
        Assert.Contains("tools/list", recordingHandler.Methods);
        Assert.Contains("tools/call", recordingHandler.Methods);
    }

    [Fact]
    public async Task Shared_client_uses_sdk_discovery_and_preserves_result_mapping()
    {
        await using var server = await CreateServerAsync();
        var recordingHandler = new RecordingHandler(server.GetTestServer().CreateHandler());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["KnowledgeAgent:Mcp:ServerName"] = "Knowledge",
                ["KnowledgeAgent:Mcp:Endpoint"] = "http://localhost/mcp"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddShared();
        services.AddHttpClient("mcp")
            .ConfigurePrimaryHttpMessageHandler(() => recordingHandler);

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IMcpToolClient>();

        var tools = await client.ListToolsAsync("Knowledge", CancellationToken.None);
        Assert.Contains(tools, tool => tool.Name == "search_knowledge_base");

        var result = await client.CallToolAsync<SearchKnowledgeResult>(
            "Knowledge",
            "search_knowledge_base",
            new
            {
                query = "refund policy",
                scope = "Sales",
                topK = 3
            },
            CancellationToken.None);

        var item = Assert.Single(Assert.IsType<SearchKnowledgeResult>(result).Items);
        Assert.Equal("Refund policy", item.DocumentTitle);
        Assert.False(recordingHandler.SawSessionHeader);
        Assert.Contains("server/discover", recordingHandler.Methods);
    }

    [Fact]
    public async Task Knowledge_tool_semantic_span_remains_above_sdk_transport()
    {
        await using var server = await CreateServerAsync();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["KnowledgeAgent:Mcp:ServerName"] = "Knowledge",
                ["KnowledgeAgent:Mcp:Endpoint"] = "http://localhost/mcp"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddShared();
        services.AddHttpClient("mcp")
            .ConfigurePrimaryHttpMessageHandler(() => server.GetTestServer().CreateHandler());

        await using var provider = services.BuildServiceProvider();
        var invoker = new KnowledgeAgentToolInvoker(
            provider.GetRequiredService<IMcpToolClient>(),
            Options.Create(new KnowledgeAgentMcpOptions { ServerName = "Knowledge" }),
            NullLogger<KnowledgeAgentToolInvoker>.Instance);
        var activities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == KnowledgeAgentTelemetry.SourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Add
        };
        ActivitySource.AddActivityListener(listener);
        using var parent = new Activity("invoke_agent BrewUp Knowledge Agent").Start();

        var result = await invoker.SearchKnowledgeBaseAsync(
            "refund policy",
            "Sales",
            Guid.Parse("01991e80-cce0-71d0-a3c8-5cf62460d633"),
            CancellationToken.None);

        var activity = Assert.Single(
            activities,
            candidate => candidate.OperationName == "execute_tool search_knowledge_base");
        Assert.NotNull(result);
        Assert.Equal(parent.SpanId, activity.ParentSpanId);
        Assert.Equal("execute_tool", activity.GetTagItem("gen_ai.operation.name"));
        Assert.Equal("search_knowledge_base", activity.GetTagItem("gen_ai.tool.name"));
        Assert.Equal("completed", activity.GetTagItem("brewup.outcome"));
    }

    private static async Task<WebApplication> CreateServerAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddMcpServer()
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools<KnowledgeTools>();
        builder.Services.AddSingleton<IKnowledgeFacade, FakeKnowledgeFacade>();

        var app = builder.Build();
        app.MapMcp("/mcp");
        await app.StartAsync();
        return app;
    }

    private sealed class FakeKnowledgeFacade : IKnowledgeFacade
    {
        public Task<object> SearchKnowledgeBaseAsync(
            SearchKnowledgeBaseRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            object result = new SearchKnowledgeResult(
            [
                new KnowledgeSearchResultItem(
                    Guid.Parse("01991e7d-9ab4-7b88-a3cd-52df26d3cd2c"),
                    Guid.Parse("01991e7d-ba05-7bdc-8edb-383d48e54f49"),
                    1,
                    "Refund policy",
                    request.Scope ?? "General",
                    ["refund"],
                    "Refunds require approval.",
                    0.95,
                    4)
            ]);

            return Task.FromResult(result);
        }
    }

    private sealed class RecordingHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
    {
        public bool SawSessionHeader { get; private set; }
        public List<string> Methods { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SawSessionHeader |= request.Headers.Contains("Mcp-Session-Id");

            if (request.Content is not null)
            {
                var body = await request.Content.ReadAsStringAsync(cancellationToken);
                using var document = System.Text.Json.JsonDocument.Parse(body);
                if (document.RootElement.TryGetProperty("method", out var method))
                    Methods.Add(method.GetString() ?? string.Empty);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
