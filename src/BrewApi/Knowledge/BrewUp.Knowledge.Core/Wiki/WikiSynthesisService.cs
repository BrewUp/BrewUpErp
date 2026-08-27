using System.Diagnostics;
using BrewUp.Knowledge.Infrastructure.Ingestion;
using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.SharedKernel.Configuration;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Wiki;
using Microsoft.Extensions.Logging;

namespace BrewUp.Knowledge.Core.Wiki;

public sealed class WikiSynthesisService(
    IWikiRepository wikiRepository,
    IKnowledgeDocumentRepository documentRepository,
    IKnowledgeChunkRepository chunkRepository,
    IWikiAnalyzer analyzer,
    IEmbeddingGenerator embeddingGenerator,
    WikiAnalysisValidator validator,
    WikiOptions options,
    ILogger<WikiSynthesisService> logger)
{
    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var job = await wikiRepository.LeaseNextJobAsync(cancellationToken);
        if (job is null)
            return false;

        using var activity = KnowledgeWikiTelemetry.Source.StartActivity(
            "knowledge.wiki.analysis",
            ActivityKind.Internal);
        activity?.SetTag("knowledge.document.id", job.DocumentId);
        activity?.SetTag("knowledge.wiki.job.id", job.Id);
        activity?.SetTag("knowledge.wiki.attempt", job.AttemptCount);

        try
        {
            var document = await documentRepository.GetByIdAsync(
                               job.DocumentId,
                               cancellationToken)
                           ?? throw new InvalidOperationException(
                               $"Knowledge document '{job.DocumentId}' no longer exists.");
            var chunks = await chunkRepository.GetByDocumentIdAsync(
                job.DocumentId,
                cancellationToken);
            var candidates = await wikiRepository.GetCandidatesAsync(
                options.CandidateLimit,
                cancellationToken);
            var context = new WikiAnalysisContext(document, chunks, candidates);
            var proposed = await analyzer.AnalyzeAsync(context, cancellationToken);
            var analysis = validator.Validate(proposed, context);
            var embeddings = await GeneratePageEmbeddingsAsync(
                analysis.Pages,
                cancellationToken);

            await wikiRepository.ApplyAnalysisAsync(
                job,
                analysis,
                embeddings,
                cancellationToken);

            activity?.SetTag("knowledge.wiki.pages.count", analysis.Pages.Count);
            activity?.SetTag("knowledge.wiki.links.count", analysis.Links.Count);
            activity?.SetTag("knowledge.wiki.issues.count", analysis.Issues.Count);
            activity?.SetTag("brewup.outcome", "completed");
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            DateTime? retryAt = job.AttemptCount < options.MaximumAttempts
                ? DateTime.UtcNow.AddSeconds(Math.Max(options.PollIntervalSeconds, 1) * job.AttemptCount)
                : null;

            await wikiRepository.MarkJobFailedAsync(
                job,
                exception.GetType().FullName ?? exception.GetType().Name,
                retryAt,
                cancellationToken);

            activity?.SetTag("brewup.outcome", "failed");
            activity?.SetTag("error.type", exception.GetType().FullName);
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(exception);
            logger.LogError(
                exception,
                "Wiki synthesis failed for document {DocumentId} on attempt {AttemptCount}",
                job.DocumentId,
                job.AttemptCount);
            return true;
        }
    }

    private async Task<IReadOnlyDictionary<string, EmbeddingVector>> GeneratePageEmbeddingsAsync(
        IReadOnlyCollection<WikiPageProposal> pages,
        CancellationToken cancellationToken)
    {
        var embeddings = new Dictionary<string, EmbeddingVector>(StringComparer.Ordinal);
        foreach (var page in pages)
        {
            embeddings[page.Key] = await embeddingGenerator.GenerateAsync(
                $"{page.Title}\n{page.Content}",
                cancellationToken);
        }

        return embeddings;
    }
}
