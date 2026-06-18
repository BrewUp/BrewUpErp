using BrewUp.Knowledge.Agent;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.Agents;

namespace BrewUp.Knowledge.Tests;

public sealed class KnowledgeAgentA2ATests
{
    [Fact]
    public void Agent_card_describes_brewup_knowledge_agent()
    {
        var provider = new BrewUpKnowledgeAgentCardProvider();

        var card = provider.GetAgentCard();

        Assert.Equal("BrewUp Knowledge Agent", card.Name);
        Assert.Contains("documented BrewUp business knowledge", card.Description);
        Assert.Contains(card.Capabilities, capability => capability.Name == "knowledge retrieval");
        Assert.Contains(card.Capabilities, capability => capability.Name == "documentation lookup");
        Assert.Contains(card.Capabilities, capability => capability.Name == "policy lookup");
        Assert.Contains(card.Capabilities, capability => capability.Name == "procedure lookup");
        Assert.Contains(card.Skills, skill => skill.Name == "search_knowledge");
        Assert.Contains("What is the reorder policy for IPA?", card.Examples);
    }

    [Fact]
    public async Task Executor_calls_search_knowledge_base_through_mcp()
    {
        var mcp = new RecordingMcpToolClient();
        var executor = new KnowledgeAgentExecutor(mcp);
        var correlationId = Guid.CreateVersion7();

        var response = await executor.ExecuteAsync(
            new A2ATaskRequest(
                "task-1",
                "What is the reorder policy for IPA?",
                correlationId,
                new Dictionary<string, object?>()),
            CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.Equal("BrewUp Knowledge Agent", response.AgentName);
        Assert.Contains("IPA reorder policy", response.Summary);
        Assert.Single(response.KnowledgeResult.Findings);
        Assert.Contains(mcp.Calls, call => call.ServerName == "knowledge"
                                           && call.ToolName == "search_knowledge_base"
                                           && call.CorrelationId == correlationId);
    }

    private sealed class RecordingMcpToolClient : IMcpToolClient
    {
        private readonly List<McpCall> _calls = [];

        public IReadOnlyCollection<McpCall> Calls => _calls;

        public Task<TResponse?> CallToolAsync<TResponse>(
            string serverName,
            string toolName,
            object arguments,
            CancellationToken cancellationToken)
        {
            var correlationId = (Guid)arguments.GetType().GetProperty("correlationId")!.GetValue(arguments)!;
            _calls.Add(new McpCall(serverName, toolName, correlationId));

            object? response = new SearchKnowledgeResult(
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

            return Task.FromResult((TResponse?)response);
        }
    }

    private sealed record McpCall(string ServerName, string ToolName, Guid CorrelationId);
}
