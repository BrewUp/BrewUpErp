namespace BrewUp.Mother.Facade.Configuration;

public sealed class FoundryLimitsOptions
{
    public const string SectionName = "BrewUp:FoundryLimits";

    // public int MaxConcurrentRequests { get; init; } = 1;
    //
    // public int QueueLimit { get; init; } = 4;
    //
    // public int RequestsPerMinute { get; init; } = 6;
    //
    // public int MaxOutputTokens { get; init; } = 600;
    //
    // public int RequestTimeoutSeconds { get; init; } = 60;
    
    public int MaxConcurrentRequests { get; init; } = 1;

    public int RequestsPerMinute { get; init; } = 6;

    public int QueueLimit { get; init; } = 4;

    public int MaxOutputTokens { get; init; } = 600;

    public int RequestTimeoutSeconds { get; init; } = 90;

    public int MaximumFunctionIterations { get; init; } = 6;

    public int MaximumConsecutiveFunctionErrors { get; init; } = 1;
}