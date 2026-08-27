using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Wiki;
using OpenAI.Chat;

namespace BrewUp.Knowledge.Infrastructure.Wiki;

internal sealed class AzureOpenAiWikiAnalyzer
{
    private static readonly BinaryData AnalysisSchema = BinaryData.FromBytes(
        """
        {
          "type": "object",
          "properties": {
            "pages": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "existingPageId": { "type": ["string", "null"] },
                  "key": { "type": "string" },
                  "title": { "type": "string" },
                  "pageType": { "type": "string" },
                  "content": { "type": "string" },
                  "scope": {
                    "type": "string",
                    "enum": ["General", "Sales", "Warehouse", "MasterData", "Production"]
                  },
                  "claims": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "key": { "type": "string" },
                        "content": { "type": "string" },
                        "evidenceChunkIds": {
                          "type": "array",
                          "items": { "type": "string" }
                        }
                      },
                      "required": ["key", "content", "evidenceChunkIds"],
                      "additionalProperties": false
                    }
                  }
                },
                "required": [
                  "existingPageId", "key", "title", "pageType", "content", "scope", "claims"
                ],
                "additionalProperties": false
              }
            },
            "links": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "sourcePageKey": { "type": "string" },
                  "targetPageKey": { "type": "string" },
                  "relationshipType": { "type": "string" }
                },
                "required": ["sourcePageKey", "targetPageKey", "relationshipType"],
                "additionalProperties": false
              }
            },
            "issues": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "pageKey": { "type": "string" },
                  "claimKey": { "type": ["string", "null"] },
                  "type": {
                    "type": "string",
                    "enum": [
                      "ContradictoryEvidence", "UnsupportedClaim", "MissingEvidence", "BrokenLink"
                    ]
                  },
                  "description": { "type": "string" }
                },
                "required": ["pageKey", "claimKey", "type", "description"],
                "additionalProperties": false
              }
            }
          },
          "required": ["pages", "links", "issues"],
          "additionalProperties": false
        }
        """u8.ToArray());

    private readonly ChatClient _chatClient;

    public AzureOpenAiWikiAnalyzer(AzureOpenAiWikiOptions options)
    {
        var clientOptions = new AzureOpenAIClientOptions
        {
            NetworkTimeout = TimeSpan.FromMinutes(3)
        };
        AzureOpenAIClient client;
        if (options.UseManagedIdentity)
        {
            TokenCredential credential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions { TenantId = options.TenantId });
            client = new AzureOpenAIClient(new Uri(options.Endpoint), credential, clientOptions);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new InvalidOperationException(
                    "BrewUp:AzureOpenAI:ApiKey is required when Wiki synthesis uses API-key authentication.");

            client = new AzureOpenAIClient(
                new Uri(options.Endpoint),
                new AzureKeyCredential(options.ApiKey),
                clientOptions);
        }

        _chatClient = client.GetChatClient(options.DeploymentName);
    }

    public async Task<WikiAnalysisResult> AnalyzeAsync(
        WikiAnalysisContext context,
        CancellationToken cancellationToken)
    {
        var source = new
        {
            document = new
            {
                context.Document.Id,
                context.Document.Title,
                scope = context.Document.Scope.Name
            },
            chunks = context.Chunks.Select(chunk => new
            {
                chunk.Id,
                chunk.Sequence,
                content = chunk.KnowledgeContent
            }),
            existingPages = context.ExistingPages
        };

        ChatMessage[] messages =
        [
            new SystemChatMessage(
                """
                You maintain BrewUp's derived LLM Wiki. Extract comparatively stable domain concepts,
                procedures, policies, terminology, relationships, and constraints. Never persist current
                operational ERP state such as stock quantities or open-order counts. Reuse an existing page
                by returning its existingPageId and key when it represents the same concept. Every claim must
                cite one or more chunk IDs supplied in this request. Record contradictions as issues instead
                of silently replacing previous knowledge. Return only the requested structured result.
                """),
            new UserChatMessage(JsonSerializer.Serialize(source))
        ];
        var completionOptions = new ChatCompletionOptions
        {
            Temperature = 0,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "wiki_analysis_result",
                AnalysisSchema,
                jsonSchemaIsStrict: true)
        };

        var completion = await _chatClient.CompleteChatAsync(
            messages,
            completionOptions,
            cancellationToken);
        var content = completion.Value.Content.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("The Wiki analyzer returned no structured content.");

        using var document = JsonDocument.Parse(content);
        return ParseResult(document.RootElement);
    }

    private static WikiAnalysisResult ParseResult(JsonElement root)
    {
        var pages = root.GetProperty("pages").EnumerateArray().Select(page =>
            new WikiPageProposal(
                page.GetProperty("existingPageId").ValueKind == JsonValueKind.String
                    ? page.GetProperty("existingPageId").GetGuid()
                    : null,
                page.GetProperty("key").GetString() ?? string.Empty,
                page.GetProperty("title").GetString() ?? string.Empty,
                page.GetProperty("pageType").GetString() ?? string.Empty,
                page.GetProperty("content").GetString() ?? string.Empty,
                DocumentScope.FromName(page.GetProperty("scope").GetString() ?? string.Empty),
                page.GetProperty("claims").EnumerateArray().Select(claim =>
                    new WikiClaimProposal(
                        claim.GetProperty("key").GetString() ?? string.Empty,
                        claim.GetProperty("content").GetString() ?? string.Empty,
                        claim.GetProperty("evidenceChunkIds").EnumerateArray()
                            .Select(item => item.GetGuid())
                            .ToArray()))
                    .ToArray()))
            .ToArray();
        var links = root.GetProperty("links").EnumerateArray().Select(link =>
            new WikiLinkProposal(
                link.GetProperty("sourcePageKey").GetString() ?? string.Empty,
                link.GetProperty("targetPageKey").GetString() ?? string.Empty,
                link.GetProperty("relationshipType").GetString() ?? string.Empty))
            .ToArray();
        var issues = root.GetProperty("issues").EnumerateArray().Select(issue =>
            new WikiIssueProposal(
                issue.GetProperty("pageKey").GetString() ?? string.Empty,
                issue.GetProperty("claimKey").ValueKind == JsonValueKind.String
                    ? issue.GetProperty("claimKey").GetString()
                    : null,
                Enum.Parse<WikiIssueType>(
                    issue.GetProperty("type").GetString() ?? string.Empty),
                issue.GetProperty("description").GetString() ?? string.Empty))
            .ToArray();
        return new WikiAnalysisResult(pages, links, issues);
    }
}

internal sealed class AzureOpenAiWikiAnalyzerAdapter(
    AzureOpenAiWikiAnalyzer analyzer) : IWikiAnalyzer
{
    public Task<WikiAnalysisResult> AnalyzeAsync(
        WikiAnalysisContext context,
        CancellationToken cancellationToken)
        => analyzer.AnalyzeAsync(context, cancellationToken);
}

