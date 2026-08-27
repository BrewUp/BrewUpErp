using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.Infrastructure.Wiki;

internal sealed class DisabledWikiAnalyzer : IWikiAnalyzer
{
    public Task<WikiAnalysisResult> AnalyzeAsync(
        WikiAnalysisContext context,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException(
            "Wiki synthesis is enabled but no LLM analyzer has been configured.");
}

