using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.ReadModel.QueryHandlers;

public sealed class GetWikiPageEvidenceHandler(IWikiRepository wikiRepository)
{
    public Task<WikiPageEvidenceResult?> HandleAsync(
        Guid pageId,
        CancellationToken cancellationToken)
        => wikiRepository.GetPageEvidenceAsync(pageId, cancellationToken);
}

