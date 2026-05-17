namespace BrewUp.Mother.Agents;

public interface IRecommendationWriter
{
    Task WriteAsync<TRecommendation>(
        TRecommendation recommendation,
        CancellationToken cancellationToken);
}