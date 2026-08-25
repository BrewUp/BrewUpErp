using System.Net.Http.Json;
using System.Text.Json;
using BrewUp.Shared.Agents;
using Microsoft.Extensions.Logging;

namespace BrewUp.Mother.Facade.Agents;

internal sealed class HttpKnowledgeAgentA2AClient(
    IHttpClientFactory httpClientFactory,
    MotherA2AOptions options,
    ILogger<HttpKnowledgeAgentA2AClient> logger) : IKnowledgeAgentA2AClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AgentCard> GetAgentCardAsync(CancellationToken cancellationToken)
    {
        var client = CreateClient();
        var card = await client.GetFromJsonAsync<AgentCard>(
            ".well-known/agent-card.json",
            JsonOptions,
            cancellationToken);

        if (card is null)
            throw new InvalidOperationException("KnowledgeAgent did not return an Agent Card.");

        logger.LogInformation("Mother discovered KnowledgeAgent through Agent Card {AgentName}", card.Name);

        return card;
    }

    public async Task<KnowledgeResult> SubmitKnowledgeTaskAsync(
        string question,
        Guid correlationId,
        string? conversationId,
        CancellationToken cancellationToken)
    {
        var client = CreateClient();
        IReadOnlyDictionary<string, object?> metadata = string.IsNullOrWhiteSpace(conversationId)
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>
            {
                [A2ATaskRequest.ConversationIdMetadataKey] = conversationId
            };

        var task = new A2ATaskRequest(
            Guid.CreateVersion7().ToString("N"),
            question,
            correlationId,
            metadata);

        logger.LogInformation(
            "Mother delegated task to KnowledgeAgent with correlation {CorrelationId}",
            correlationId);

        using var response = await client.PostAsJsonAsync(
            "a2a/tasks",
            task,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var taskResponse = await response.Content.ReadFromJsonAsync<A2ATaskResponse>(
            JsonOptions,
            cancellationToken);

        return taskResponse?.KnowledgeResult
               ?? new KnowledgeResult([]);
    }

    private HttpClient CreateClient()
    {
        if (string.IsNullOrWhiteSpace(options.KnowledgeAgentUrl))
            throw new InvalidOperationException("Mother:A2A:KnowledgeAgentUrl is required when Mother:A2A:Enabled is true.");

        var client = httpClientFactory.CreateClient("a2a-knowledge");
        client.BaseAddress ??= new Uri(options.KnowledgeAgentUrl.TrimEnd('/') + "/");
        return client;
    }
}
