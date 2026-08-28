using System.Diagnostics;
using System.Text.Json;
using BrewUp.Knowledge.Core;
using BrewUp.Knowledge.Core.CommandHandlers;
using BrewUp.Knowledge.Core.Documents;
using BrewUp.Knowledge.Core.Wiki;
using BrewUp.Knowledge.Facade.Governance;
using BrewUp.Knowledge.Infrastructure;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.ReadModel;
using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Chunks;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.CustomTypes;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Messages.Commands;
using BrewUp.Knowledge.SharedKernel.Wiki;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.Knowledge.Tests;

public sealed class KnowledgeWikiTests
{
    [Fact]
    public void Wiki_analysis_parser_treats_an_empty_existing_page_id_as_a_new_page()
    {
        var chunkId = Guid.NewGuid();
        using var document = JsonDocument.Parse(
            $$"""
              {
                "pages": [
                  {
                    "existingPageId": "",
                    "key": "ipa",
                    "title": "IPA",
                    "pageType": "DomainConcept",
                    "content": "IPA is a hop-forward beer style.",
                    "scope": "Production",
                    "claims": [
                      {
                        "key": "hop-forward",
                        "content": "IPA is hop-forward.",
                        "evidenceChunkIds": ["{{chunkId}}"]
                      }
                    ]
                  }
                ],
                "links": [],
                "issues": []
              }
              """);

        var result = AzureOpenAiWikiAnalyzer.ParseResult(document.RootElement);

        Assert.Null(Assert.Single(result.Pages).ExistingPageId);
        Assert.Equal(chunkId, Assert.Single(Assert.Single(result.Pages).Claims).EvidenceChunkIds.Single());
    }

    [Fact]
    public void Wiki_analysis_parser_reports_a_non_guid_existing_page_id()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "pages": [
                {
                  "existingPageId": "ipa",
                  "key": "ipa",
                  "title": "IPA",
                  "pageType": "DomainConcept",
                  "content": "IPA is a hop-forward beer style.",
                  "scope": "Production",
                  "claims": []
                }
              ],
              "links": [],
              "issues": []
            }
            """);

        var exception = Assert.Throws<JsonException>(
            () => AzureOpenAiWikiAnalyzer.ParseResult(document.RootElement));

        Assert.Contains("existingPageId", exception.Message);
        Assert.Contains("ipa", exception.Message);
    }

    [Fact]
    public async Task Ingested_documents_incrementally_create_and_enrich_traceable_pages()
    {
        var analyzer = new RecordingWikiAnalyzer(context =>
        {
            var existing = context.ExistingPages.SingleOrDefault(page => page.NormalizedKey == "ipa");
            var chunkId = Assert.Single(context.Chunks).Id;
            return new WikiAnalysisResult(
            [
                new WikiPageProposal(
                    existing?.Id,
                    "IPA",
                    "IPA",
                    "DomainConcept",
                    existing is null
                        ? "IPA is a hop-forward beer style."
                        : "IPA is a hop-forward beer style commonly produced with dry hopping.",
                    DocumentScope.Production,
                    [
                        new WikiClaimProposal(
                            existing is null ? "hop-forward" : "dry-hop-aroma",
                            existing is null
                                ? "IPA is hop-forward."
                                : "Dry hopping adds aroma to IPA.",
                            [chunkId])
                    ]),
                new WikiPageProposal(
                    null,
                    "Dry Hopping",
                    "Dry Hopping",
                    "Procedure",
                    "Dry hopping adds hop aroma after the boil.",
                    DocumentScope.Production,
                    [
                        new WikiClaimProposal(
                            "adds-aroma",
                            "Dry hopping adds hop aroma.",
                            [chunkId])
                    ])
            ],
            [
                new WikiLinkProposal("IPA", "Dry Hopping", "uses")
            ],
            existing is null
                ? []
                :
                [
                    new WikiIssueProposal(
                        "IPA",
                        "hop-forward",
                        WikiIssueType.ContradictoryEvidence,
                        "The new source describes a different hopping emphasis.")
                ]);
        });
        await using var provider = CreateProvider(analyzer);
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        var first = await IngestAsync(
            provider,
            "IPA guide",
            "IPA is a hop-forward beer style.");
        Assert.Equal(WikiProcessingStatus.Pending, first.WikiStatus);
        await ProcessNextAsync(provider);

        var pageHandler = services.GetRequiredService<GetWikiPageHandler>();
        var firstPage = Assert.IsType<WikiPageResult>(
            await pageHandler.HandleAsync("ÍPA", CancellationToken.None));
        Assert.Equal(1, firstPage.Page.CurrentRevision);
        Assert.Single(firstPage.Claims);
        Assert.Single(firstPage.Links);

        var evidenceHandler = services.GetRequiredService<GetWikiPageEvidenceHandler>();
        var evidence = Assert.IsType<WikiPageEvidenceResult>(
            await evidenceHandler.HandleAsync(firstPage.Page.Id, CancellationToken.None));
        var source = Assert.Single(evidence.Evidence);
        Assert.Equal(first.DocumentId, source.Evidence.DocumentId);
        Assert.Equal(WikiEvidenceStatus.Available, source.Evidence.Status);
        Assert.Contains("hop-forward", source.SourceContent);

        var second = await IngestAsync(
            provider,
            "Dry hopping guide",
            "IPA commonly uses dry hopping to add aroma.");
        await ProcessNextAsync(provider);

        var enriched = Assert.IsType<WikiPageResult>(
            await pageHandler.HandleAsync("ipa", CancellationToken.None));
        Assert.Equal(firstPage.Page.Id, enriched.Page.Id);
        Assert.Equal(2, enriched.Page.CurrentRevision);
        Assert.Equal(2, enriched.Claims.Count);
        Assert.Single(enriched.Issues);
        Assert.Equal(WikiIssueType.ContradictoryEvidence, enriched.Issues.Single().Type);
        var enrichedEvidence = Assert.IsType<WikiPageEvidenceResult>(
            await evidenceHandler.HandleAsync(enriched.Page.Id, CancellationToken.None));
        Assert.Contains(
            enrichedEvidence.Evidence,
            item => item.Evidence.DocumentId == first.DocumentId);
        Assert.Contains(
            enrichedEvidence.Evidence,
            item => item.Evidence.DocumentId == second.DocumentId);

        var repository = services.GetRequiredService<IWikiRepository>();
        var completed = await repository.GetJobByDocumentIdAsync(
            second.DocumentId,
            CancellationToken.None);
        Assert.Equal(WikiProcessingStatus.Completed, completed!.Status);

        var query = await services.GetRequiredService<QueryWikiHandler>().HandleAsync(
            new QueryWiki("dry hopping", "Production", 5),
            CancellationToken.None);
        Assert.Contains(query.Items, item => item.Key == "ipa");
    }

    [Fact]
    public async Task Deleting_document_preserves_page_and_marks_evidence_unavailable()
    {
        var analyzer = new RecordingWikiAnalyzer(context =>
            SinglePageAnalysis(context, "Fermentation Temperature"));
        await using var provider = CreateProvider(analyzer);
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var ingested = await IngestAsync(
            provider,
            "Fermentation",
            "Ale fermentation temperature should remain stable.");
        await ProcessNextAsync(provider);

        var page = Assert.IsType<WikiPageResult>(
            await services.GetRequiredService<GetWikiPageHandler>()
                .HandleAsync("fermentation-temperature", CancellationToken.None));
        var deleted = await services.GetRequiredService<DeleteKnowledgeDocumentHandler>()
            .HandleAsync(ingested.DocumentId, CancellationToken.None);
        var evidence = Assert.IsType<WikiPageEvidenceResult>(
            await services.GetRequiredService<GetWikiPageEvidenceHandler>()
                .HandleAsync(page.Page.Id, CancellationToken.None));

        Assert.True(deleted);
        Assert.Equal(WikiEvidenceStatus.Unavailable, Assert.Single(evidence.Evidence).Evidence.Status);
        Assert.Null(Assert.Single(evidence.Evidence).SourceContent);
        Assert.NotNull(await services.GetRequiredService<GetWikiPageHandler>()
            .HandleAsync("fermentation-temperature", CancellationToken.None));
    }

    [Fact]
    public async Task Analyzer_failure_marks_wiki_job_failed_without_invalidating_rag()
    {
        var analyzer = new ThrowingWikiAnalyzer();
        await using var provider = CreateProvider(analyzer, maximumAttempts: 1);
        var activities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == KnowledgeWikiTelemetry.SourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Add
        };
        ActivitySource.AddActivityListener(listener);
        var ingested = await IngestAsync(provider, "Policy", "A stable production policy.");
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        await ProcessNextAsync(provider);

        var document = await services.GetRequiredService<IKnowledgeDocumentRepository>()
            .GetByIdAsync(ingested.DocumentId, CancellationToken.None);
        var job = await services.GetRequiredService<IWikiRepository>()
            .GetJobByDocumentIdAsync(ingested.DocumentId, CancellationToken.None);
        Assert.NotNull(document);
        Assert.Equal(WikiProcessingStatus.Failed, job!.Status);
        Assert.Equal(typeof(InvalidOperationException).FullName, job.ErrorType);
        var activity = Assert.Single(activities);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("failed", activity.GetTagItem("brewup.outcome"));
    }

    [Fact]
    public async Task Reindex_creates_a_new_job_generation_and_resynthesizes()
    {
        var analyzer = new RecordingWikiAnalyzer(context =>
            SinglePageAnalysis(context, "Brewhouse Safety"));
        await using var provider = CreateProvider(analyzer);
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var ingested = await IngestAsync(
            provider,
            "Brewhouse safety",
            "Wear protective equipment in the brewhouse.");
        await ProcessNextAsync(provider);

        var repository = services.GetRequiredService<IWikiRepository>();
        var firstJob = Assert.IsType<WikiProcessingJob>(
            await repository.GetJobByDocumentIdAsync(
                ingested.DocumentId,
                CancellationToken.None));
        var result = await services.GetRequiredService<ReindexKnowledgeDocumentHandler>()
            .HandleAsync(ingested.DocumentId, CancellationToken.None);
        var secondJob = Assert.IsType<WikiProcessingJob>(
            await repository.GetJobByDocumentIdAsync(
                ingested.DocumentId,
                CancellationToken.None));

        Assert.NotNull(result);
        Assert.Equal(WikiProcessingStatus.Pending, result.WikiStatus);
        Assert.NotEqual(firstJob.Id, secondJob.Id);
        Assert.Equal(0, secondJob.AttemptCount);

        await ProcessNextAsync(provider);

        var page = Assert.IsType<WikiPageResult>(
            await services.GetRequiredService<GetWikiPageHandler>()
                .HandleAsync("brewhouse-safety", CancellationToken.None));
        Assert.Equal(2, page.Page.CurrentRevision);
        Assert.Equal(
            WikiProcessingStatus.Completed,
            (await repository.GetJobByDocumentIdAsync(
                ingested.DocumentId,
                CancellationToken.None))!.Status);
    }

    [Fact]
    public void Validator_rejects_unknown_evidence_and_broken_links()
    {
        var context = CreateAnalysisContext();
        var validator = new WikiAnalysisValidator(new WikiOptions { Enabled = true });
        var unknownEvidence = SinglePageAnalysis(context, "IPA") with
        {
            Pages =
            [
                SinglePageAnalysis(context, "IPA").Pages.Single() with
                {
                    Claims =
                    [
                        new WikiClaimProposal("claim", "Claim", [Guid.CreateVersion7()])
                    ]
                }
            ]
        };
        var brokenLink = SinglePageAnalysis(context, "IPA") with
        {
            Links = [new WikiLinkProposal("IPA", "Unknown page", "related")]
        };

        Assert.Throws<InvalidOperationException>(() => validator.Validate(unknownEvidence, context));
        Assert.Throws<InvalidOperationException>(() => validator.Validate(brokenLink, context));
    }

    [Fact]
    public void Validator_allows_stable_inventory_reorder_policy()
    {
        var context = CreateAnalysisContext();
        var validator = new WikiAnalysisValidator(new WikiOptions { Enabled = true });
        var analysis = SinglePageAnalysis(context, "Inventory reorder policy");
        var page = analysis.Pages.Single() with
        {
            Content =
                "The inventory reorder policy starts replenishment when current stock falls below the reorder threshold.",
            Claims =
            [
                analysis.Pages.Single().Claims.Single() with
                {
                    Content = "Replenishment starts when stock on hand reaches the reorder threshold."
                }
            ]
        };

        var result = validator.Validate(
            analysis with { Pages = [page] },
            context);

        Assert.Single(result.Pages);
    }

    [Theory]
    [InlineData("Current stock is 42 units.")]
    [InlineData("There are 5 open sales orders.")]
    [InlineData("Currently available: 18 units.")]
    [InlineData("Live inventory = 27.")]
    public void Validator_rejects_concrete_operational_state(string content)
    {
        var context = CreateAnalysisContext();
        var validator = new WikiAnalysisValidator(new WikiOptions { Enabled = true });
        var analysis = SinglePageAnalysis(context, "Inventory status");
        var page = analysis.Pages.Single() with { Content = content };

        var exception = Assert.Throws<InvalidOperationException>(
            () => validator.Validate(analysis with { Pages = [page] }, context));

        Assert.Contains("operational ERP state", exception.Message);
    }

    [Fact]
    public void Validator_rejects_concrete_operational_state_in_claims()
    {
        var context = CreateAnalysisContext();
        var validator = new WikiAnalysisValidator(new WikiOptions { Enabled = true });
        var analysis = SinglePageAnalysis(context, "Inventory status");
        var page = analysis.Pages.Single();
        var claim = page.Claims.Single() with { Content = "Stock on hand equals 12 units." };

        var exception = Assert.Throws<InvalidOperationException>(
            () => validator.Validate(
                analysis with
                {
                    Pages = [page with { Claims = [claim] }]
                },
                context));

        Assert.Contains("operational ERP state", exception.Message);
    }

    private static async Task ProcessNextAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        Assert.True(await scope.ServiceProvider.GetRequiredService<WikiSynthesisService>()
            .ProcessNextAsync(CancellationToken.None));
    }

    private static async Task<IngestKnowledgeDocumentResult> IngestAsync(
        ServiceProvider provider,
        string title,
        string content)
    {
        using var scope = provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IngestKnowledgeDocumentHandler>()
            .HandleAsync(
                new IngestKnowledgeDocument(
                    title,
                    content,
                    DocumentScope.Production,
                    DocumentSource.PlainText),
                CancellationToken.None);
    }

    private static ServiceProvider CreateProvider(
        IWikiAnalyzer analyzer,
        int maximumAttempts = 3)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCore();
        services.AddInfrastructure();
        services.AddKnowledgeReadModel();
        services.AddScoped<DeleteKnowledgeDocumentHandler>();
        services.AddScoped<ReindexKnowledgeDocumentHandler>();
        services.AddSingleton(new WikiOptions
        {
            Enabled = true,
            MaximumAttempts = maximumAttempts,
            PollIntervalSeconds = 1
        });
        services.AddSingleton(analyzer);
        services.AddSingleton<IWikiAnalyzer>(analyzer);
        services.AddSingleton<IEmbeddingGenerator, TestEmbeddingGenerator>();
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static WikiAnalysisResult SinglePageAnalysis(
        WikiAnalysisContext context,
        string title)
    {
        var chunk = Assert.Single(context.Chunks);
        return new WikiAnalysisResult(
        [
            new WikiPageProposal(
                context.ExistingPages.SingleOrDefault()?.Id,
                title,
                title,
                "DomainConcept",
                chunk.KnowledgeContent,
                DocumentScope.Production,
                [
                    new WikiClaimProposal("claim", chunk.KnowledgeContent, [chunk.Id])
                ])
        ],
        [],
        []);
    }

    private static WikiAnalysisContext CreateAnalysisContext()
    {
        var documentId = Guid.CreateVersion7();
        var document = new KnowledgeDocument
        {
            Id = documentId,
            Title = "Test",
            DocumentsContent = "Evidence",
            Scope = DocumentScope.Production,
            Source = DocumentSource.PlainText,
            ImportedAt = DateTime.UtcNow
        };
        var chunk = new KnowledgeChunk
        {
            Id = Guid.CreateVersion7(),
            DocumentId = documentId,
            KnowledgeContent = "Evidence",
            Sequence = 0,
            Metadata = new ChunkMetadata
            {
                Title = document.Title,
                Scope = document.Scope
            }
        };
        return new WikiAnalysisContext(document, [chunk], []);
    }

    private sealed class RecordingWikiAnalyzer(
        Func<WikiAnalysisContext, WikiAnalysisResult> analyze) : IWikiAnalyzer
    {
        public Task<WikiAnalysisResult> AnalyzeAsync(
            WikiAnalysisContext context,
            CancellationToken cancellationToken)
            => Task.FromResult(analyze(context));
    }

    private sealed class ThrowingWikiAnalyzer : IWikiAnalyzer
    {
        public Task<WikiAnalysisResult> AnalyzeAsync(
            WikiAnalysisContext context,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Invalid structured output.");
    }

    private sealed class TestEmbeddingGenerator : IEmbeddingGenerator
    {
        public Task<EmbeddingVector> GenerateAsync(
            string text,
            CancellationToken cancellationToken)
        {
            var normalized = text.ToLowerInvariant();
            return Task.FromResult(new EmbeddingVector(
            [
                normalized.Contains("ipa") ? 1 : 0,
                normalized.Contains("dry hopping") ? 1 : 0,
                normalized.Contains("fermentation") ? 1 : 0
            ]));
        }
    }
}
