using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.Infrastructure.Wiki;

public interface IWikiAnalyzer
{
    Task<WikiAnalysisResult> AnalyzeAsync(
        WikiAnalysisContext context,
        CancellationToken cancellationToken);
}

