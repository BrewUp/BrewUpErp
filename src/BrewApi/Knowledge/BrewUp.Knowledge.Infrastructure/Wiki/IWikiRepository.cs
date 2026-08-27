using BrewUp.Knowledge.SharedKernel.Embeddings;
using BrewUp.Knowledge.SharedKernel.Enums;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.Infrastructure.Wiki;

public interface IWikiRepository
{
    Task<WikiProcessingStatus> EnqueueAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task<int> EnqueueMissingDocumentsAsync(CancellationToken cancellationToken);

    Task<WikiProcessingJob?> LeaseNextJobAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<WikiPageCandidate>> GetCandidatesAsync(
        int limit,
        CancellationToken cancellationToken);

    Task ApplyAnalysisAsync(
        WikiProcessingJob job,
        WikiAnalysisResult analysis,
        IReadOnlyDictionary<string, EmbeddingVector> pageEmbeddings,
        CancellationToken cancellationToken);

    Task MarkJobFailedAsync(
        WikiProcessingJob job,
        string errorType,
        DateTime? nextAttemptAt,
        CancellationToken cancellationToken);

    Task MarkEvidenceUnavailableAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task<WikiProcessingJob?> GetJobByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task<WikiPageResult?> GetPageAsync(
        string normalizedKey,
        CancellationToken cancellationToken);

    Task<WikiPageEvidenceResult?> GetPageEvidenceAsync(
        Guid pageId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<(WikiPage Page, double Score)>> SearchAsync(
        EmbeddingVector queryEmbedding,
        DocumentScope? scope,
        int topK,
        CancellationToken cancellationToken);
}
