namespace Be.Haven.Cache.Behaviours;

/// <summary>
/// Mediator pipeline behavior that invalidates (logically) cached query results after successful write operations.
///
/// Strategy:
/// - Resolve request-specific invalidation targets through server-side invalidation policies.
/// - On successful request execution, bump the scoped cache version for each declared target.
/// - Queries include {epoch, version} in their cache key, so a bumped version automatically forces a new cache key,
///   preventing stale reads without scanning/deleting existing keys.
/// </summary>
public class InvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly ICacheVersionService _version;
    private readonly ILogger<InvalidationBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<ICacheInvalidationPolicy<TRequest>> _policies;
    private readonly ICacheBypassService _cacheBypassService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="version">The scoped cache version service.</param>
    /// <param name="logger">The cache invalidation behavior logger.</param>
    /// <param name="policies">The server-side invalidation policies for the current request type.</param>
    /// <param name="cacheBypassService">The registry used to skip stale reads when invalidation cannot be guaranteed.</param>
    public InvalidationBehavior(
        ICacheVersionService version,
        ILogger<InvalidationBehavior<TRequest, TResponse>> logger,
        IEnumerable<ICacheInvalidationPolicy<TRequest>> policies,
        ICacheBypassService cacheBypassService)
    {
        _version = version;
        _logger = logger;
        _policies = policies;
        _cacheBypassService = cacheBypassService;
    }

    /// <summary>
    /// Handles the execution of the pipeline behavior by bumping scoped cache versions
    /// for the targets declared by the resolved invalidation policies.
    /// </summary>
    /// <param name="message">The request object of type <typeparamref name="TRequest"/> to be processed.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline.</param>
    /// <param name="cancellationToken">A token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation, resolving to a response of type <typeparamref name="TResponse"/>.</returns>
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Execute the request first.
        // Only bump version if the request completes successfully (no exception thrown).
        var response = await next(message, cancellationToken);

        var targets = _policies
            .SelectMany(policy => policy.GetTargets(message))
            .Where(target => !string.IsNullOrWhiteSpace(target.CacheGroup))
            .Distinct()
            .ToArray();

        // Requests without invalidation policies are treated as cache-neutral operations.
        if (targets.Length == 0)
        {
            return response;
        }

        var epoch = _version.GetEpoch();
        foreach (var target in targets)
        {
            await InvalidateTargetAsync(target, epoch, cancellationToken);
        }

        return response;
    }

    /// <summary>
    /// Invalidates one cache target by advancing its version namespace.
    /// </summary>
    /// <param name="target">The logical cache target being invalidated.</param>
    /// <param name="epoch">The captured cache epoch.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    private async Task InvalidateTargetAsync(
        CacheInvalidationTargetModel target,
        string epoch,
        CancellationToken cancellationToken)
    {
        long newVersion;
        try
        {
            // Version bump is the correctness barrier; stale payloads under old versions can expire naturally.
            newVersion = await _version.InvalidateAsync(target.CacheGroup, target.CacheScope, epoch);
        }
        catch (Exception ex)
        {
            await MarkBypassAfterInvalidationFailureAsync(target, ex, cancellationToken);
            return;
        }

        if (newVersion <= DEFAULT_VERSION)
        {
            var invalidVersionException = new InvalidOperationException(string.Format(
                CacheVersionLogs.CACHE_VERSION_NOT_ADVANCED,
                target.CacheGroup,
                target.CacheScope,
                epoch,
                DEFAULT_VERSION,
                newVersion));

            await MarkBypassAfterInvalidationFailureAsync(target, invalidVersionException, cancellationToken);
            return;
        }

        _logger.LogInformation(
            CacheLogs.CACHE_VERSION_BUMPED,
            target.CacheGroup,
            target.CacheScope,
            epoch,
            newVersion);
    }

    /// <summary>
    /// Logs an invalidation failure and marks critical targets for temporary cache bypass.
    /// </summary>
    /// <param name="target">The logical cache target being invalidated.</param>
    /// <param name="exception">The invalidation failure.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
    private async Task MarkBypassAfterInvalidationFailureAsync(
        CacheInvalidationTargetModel target,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            exception,
            CacheLogs.CACHE_TARGET_INVALIDATION_FAILED,
            typeof(TRequest).Name,
            target.CacheGroup,
            target.CacheScope);

        if (target.FailOnError)
        {
            // Bypass is fail-safe: future reads should hit the handler instead of a potentially stale payload.
            await _cacheBypassService.MarkBypassAsync(target.CacheGroup, target.CacheScope, cancellationToken);
        }
    }
}
