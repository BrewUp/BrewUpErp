using BrewUp.Knowledge.Infrastructure.Wiki;
using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.ReadModel.QueryHandlers;

public sealed class GetWikiPageHandler(IWikiRepository wikiRepository)
{
    public Task<WikiPageResult?> HandleAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var normalizedKey = WikiKeyNormalizer.Normalize(key);
        if (string.IsNullOrWhiteSpace(normalizedKey))
            throw new ArgumentException("A Wiki page key is required.", nameof(key));

        return wikiRepository.GetPageAsync(normalizedKey, cancellationToken);
    }
}

