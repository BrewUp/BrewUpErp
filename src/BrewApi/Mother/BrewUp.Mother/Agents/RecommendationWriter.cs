namespace BrewUp.Mother.Agents;

internal sealed class RecommendationWriter(
    ILoggerFactory loggerFactory) : IRecommendationWriter
{
    private readonly ILogger<RecommendationWriter> _logger = loggerFactory.CreateLogger<RecommendationWriter>();
    
    public Task WriteAsync<TRecommendation>(TRecommendation recommendation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        _logger.LogWarning(
            "BrewUp.Mother recommendation created: {@Recommendation}",
            recommendation);
        
        return Task.CompletedTask;
    }
}