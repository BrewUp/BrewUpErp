using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.ReadModel.QueryHandlers;

public sealed class GetWikiProcessingJobHandler(IWikiRepository wikiRepository)
{
    public async Task<WikiProcessingJobResult?> HandleAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var job = await wikiRepository.GetJobByDocumentIdAsync(documentId, cancellationToken);
        return job is null
            ? null
            : new WikiProcessingJobResult(
                job.DocumentId,
                job.Status,
                job.AttemptCount,
                job.ErrorType,
                job.UpdatedAt);
    }
}

