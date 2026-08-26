using System.Diagnostics;
using BrewUp.Mother.Facade.Agents;
using BrewUp.Mother.Facade.Mcp;
using BrewUp.Mother.Facade.Telemetry;
using BrewUp.Mother.SharedKernel.Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ChatResponse = BrewUp.Mother.SharedKernel.Chat.ChatResponse;

namespace BrewUp.Mother.Facade.Chat;

public sealed class BrewUpChatService(
    IChatClient chatClient,
    IMcpToolsProvider mcpToolsProvider,
    MotherCoordinator motherCoordinator,
    ILogger<BrewUpChatService> logger)
{
    private static readonly string SystemPromptPath =
        Path.Combine(AppContext.BaseDirectory, "Prompts", "Agent.md");

    private static string LoadSystemPrompt()
    {
        if (!File.Exists(SystemPromptPath))
            throw new FileNotFoundException(
                $"System prompt file '{SystemPromptPath}' was not found.", SystemPromptPath);

        return File.ReadAllText(SystemPromptPath);
    }

    public async Task<ChatResponse> AskAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        AgentRunContext run = new(Guid.CreateVersion7(), request.ConversationId);
        var route = motherCoordinator.GetRoute(request);
        var startedAt = Stopwatch.GetTimestamp();

        using var activity = MotherTelemetry.Source.StartActivity("AgentRun");
        activity?.SetTag("brewup.agent_run.id", run.RunId);
        activity?.SetTag("brewup.route", route);
        if (!string.IsNullOrWhiteSpace(run.ConversationId))
            activity?.SetTag("gen_ai.conversation.id", run.ConversationId);

        try
        {
            if (motherCoordinator.CanCoordinate(request))
                return await motherCoordinator.CoordinateAsync(request, run, cancellationToken);

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, LoadSystemPrompt()),
                new(ChatRole.User, request.Message)
            };

            IReadOnlyList<AITool> tools;
            try
            {
                tools = await mcpToolsProvider.GetToolsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load MCP tools.");
                tools = [];
            }

            var options = new ChatOptions
            {
                Tools = tools.Count > 0 ? tools.ToList() : null
            };

            var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);
            return new ChatResponse(response.Text, request.ConversationId);
        }
        catch (Exception ex)
        {
            run.SetOutcome("failed");
            activity?.SetTag("error.type", ex.GetType().FullName);
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            logger.LogError(ex, "Agent run failed for conversation {ConversationId}.", request.ConversationId);
            throw;
        }
        finally
        {
            activity?.SetTag("brewup.outcome", run.Outcome);

            TagList tags = new()
            {
                { "route", route },
                { "outcome", run.Outcome }
            };

            MotherTelemetry.AgentRuns.Add(1, tags);
            MotherTelemetry.AgentRunDuration.Record(
                Stopwatch.GetElapsedTime(startedAt).TotalSeconds,
                tags);
        }
    }
}
