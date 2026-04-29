namespace Haven.Cache.Behaviours;

/// <summary>
/// MediatR pipeline behavior that invalidates (logically) cached query results after successful write operations.
///
/// Strategy:
/// - Detect write requests by naming convention: request type name ends with "Command".
/// - On successful command execution, bump the global cache version for the current epoch (INCR in Redis).
/// - Queries include {epoch, version} in their cache key, so a bumped version automatically forces a new cache key,
///   preventing stale reads without scanning/deleting existing keys.
/// </summary>
public class InvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICacheVersionService _version;
    private readonly ILogger<InvalidationBehavior<TRequest, TResponse>> _logger;

    public InvalidationBehavior(
        ICacheVersionService version,
        ILogger<InvalidationBehavior<TRequest, TResponse>> logger)
    {
        _version = version;
        _logger = logger;
    }

    /// <summary>
    /// Handles the execution of the pipeline behavior, invalidating cached query results during the processing
    /// of write operations (commands) by bumping the global cache version upon successful execution.
    /// </summary>
    /// <param name="request">The request object of type <typeparamref name="TRequest"/> to be processed.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline.</param>
    /// <param name="cancellationToken">A token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation, resolving to a response of type <typeparamref name="TResponse"/>.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Execute the request first.
        // Only bump version if the request completes successfully (no exception thrown).
        var response = await next(cancellationToken);

        // Only invalidate after successful WRITE operations (Commands)
        if (!typeof(TRequest).Name.EndsWith(COMMAND_PREFIX, StringComparison.Ordinal))
            return response;

        try
        {
            // Bump version using Redis INCR (atomic). This makes all subsequent query cache keys change.
            var epoch = _version.GetEpoch();
            var newVersion = await _version.InvalidateAsync(epoch);

            _logger.LogInformation(
                CacheLogs.CACHE_VERSION_BUMPED,
                epoch,
                newVersion);
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