using Be.Haven.Cache.Interfaces;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="version">The scoped cache version service.</param>
    /// <param name="logger">The cache invalidation behavior logger.</param>
    /// <param name="policies">The server-side invalidation policies for the current request type.</param>
    public InvalidationBehavior(
        ICacheVersionService version,
        ILogger<InvalidationBehavior<TRequest, TResponse>> logger,
        IEnumerable<ICacheInvalidationPolicy<TRequest>> policies)
    {
        _version = version;
        _logger = logger;
        _policies = policies;
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

        try
        {
            var epoch = _version.GetEpoch();
            foreach (var target in targets)
            {
                // Each target maintains its own version namespace, so only affected scoped caches miss after this request.
                var newVersion = await _version.InvalidateAsync(target.CacheGroup, target.CacheScope, epoch);

                _logger.LogInformation(
                    CacheLogs.CACHE_VERSION_BUMPED,
                    $"{target.CacheGroup}{(string.IsNullOrWhiteSpace(target.CacheScope) ? string.Empty : string.Format(CACHE_SCOPE_SEGMENT_FORMAT, target.CacheScope))}:{epoch}",
                    newVersion);
            }
        }
        catch (Exception ex)
        {
            // Fail-open: version bump failure should not fail the command.
            // Worst case is stale cache until TTL expires.
            _logger.LogWarning(
                ex,
                CacheLogs.CACHE_INVALIDATION_FAILED,
                typeof(TRequest).Name);
        }

        return response;
    }
}
