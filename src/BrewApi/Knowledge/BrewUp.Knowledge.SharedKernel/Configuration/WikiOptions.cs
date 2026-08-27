namespace BrewUp.Knowledge.SharedKernel.Configuration;

public sealed class WikiOptions
{
    public const string SectionName = "Knowledge:Wiki";

    public bool Enabled { get; init; }
    public int PollIntervalSeconds { get; init; } = 5;
    public int LeaseDurationSeconds { get; init; } = 300;
    public int CandidateLimit { get; init; } = 25;
    public int MaximumAttempts { get; init; } = 3;
    public int MaximumPagesPerAnalysis { get; init; } = 20;
    public int MaximumClaimsPerPage { get; init; } = 30;
    public int MaximumContentLength { get; init; } = 20_000;
}
