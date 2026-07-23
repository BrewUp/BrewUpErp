using BrewUp.Mother.Facade.Configuration;
using Microsoft.Extensions.Logging;

namespace BrewUp.Mother.Facade.Foundry;

using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using Microsoft.Extensions.AI;

public sealed class FoundryGuardedChatClient : DelegatingChatClient
{
    private readonly RateLimiter _requestRateLimiter;
    private readonly RateLimiter _concurrencyLimiter;
    private readonly FoundryLimitsOptions _options;

    public FoundryGuardedChatClient(
        IChatClient innerClient,
        FoundryLimitsOptions options,
        ILogger<FoundryGuardedChatClient> logger)
        : base(innerClient)
    {
        ArgumentNullException.ThrowIfNull(innerClient);
        ArgumentNullException.ThrowIfNull(options);

        if (options.RequestsPerMinute <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.RequestsPerMinute));
        }

        _options = options;

        _concurrencyLimiter = new ConcurrencyLimiter(
            new ConcurrencyLimiterOptions
            {
                PermitLimit = options.MaxConcurrentRequests,
                QueueLimit = options.QueueLimit,
                QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst
            });

        var interval = TimeSpan.FromSeconds(
            60d / options.RequestsPerMinute);

        _requestRateLimiter = new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                // Consente al massimo un piccolo burst iniziale.
                TokenLimit = 2,

                TokensPerPeriod = 1,
                ReplenishmentPeriod = interval,
                AutoReplenishment = true,

                QueueLimit = options.QueueLimit,
                QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst
            });
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var rateLease = await AcquireAsync(
            _requestRateLimiter,
            cancellationToken);

        using var concurrencyLease = await AcquireAsync(
            _concurrencyLimiter,
            cancellationToken);

        using var timeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutSource.CancelAfter(
            TimeSpan.FromSeconds(
                _options.RequestTimeoutSeconds));

        var effectiveOptions = ApplyLimits(options);

        return await base.GetResponseAsync(
            messages,
            effectiveOptions,
            timeoutSource.Token);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate>
        GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        using var rateLease = await AcquireAsync(
            _requestRateLimiter,
            cancellationToken);

        using var concurrencyLease = await AcquireAsync(
            _concurrencyLimiter,
            cancellationToken);

        using var timeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutSource.CancelAfter(
            TimeSpan.FromSeconds(
                _options.RequestTimeoutSeconds));

        var effectiveOptions = ApplyLimits(options);

        await foreach (var update in
            base.GetStreamingResponseAsync(
                    messages,
                    effectiveOptions,
                    timeoutSource.Token)
                .WithCancellation(timeoutSource.Token)
                .ConfigureAwait(false))
        {
            yield return update;
        }
    }

    private ChatOptions ApplyLimits(ChatOptions? source)
    {
        var options = source?.Clone() ?? new ChatOptions();

        options.MaxOutputTokens = Math.Min(
            options.MaxOutputTokens
                ?? _options.MaxOutputTokens,
            _options.MaxOutputTokens);

        return options;
    }

    private static async Task<RateLimitLease> AcquireAsync(
        RateLimiter limiter,
        CancellationToken cancellationToken)
    {
        var lease = await limiter.AcquireAsync(
            permitCount: 1,
            cancellationToken);

        if (!lease.IsAcquired)
        {
            lease.Dispose();

            throw new FoundryLocalRateLimitException(
                "The local Foundry request limit was exceeded.");
        }

        return lease;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _requestRateLimiter.Dispose();
            _concurrencyLimiter.Dispose();
        }

        base.Dispose(disposing);
    }
}

public sealed class FoundryLocalRateLimitException(
    string message)
    : Exception(message);