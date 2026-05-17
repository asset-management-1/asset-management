namespace Be.Haven.Cache.Behaviours;

/// <summary>
/// Mediator pipeline behavior that caches responses for cacheable query requests.
///
/// Key points:
/// - Only applies to requests implementing <see cref="ICacheableMediatorQueryService"/> (queries).
/// - Uses an epoch + version strategy to avoid stale data after writes:
///   CacheKey := "{GroupCacheKey}:e{epoch}:v{version}:{paramsHash}"
/// - The version is expected to be bumped by a separate behavior after successful commands,
///   so subsequent queries automatically read/write a new cache key and never return stale data.
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICacheableMediatorQueryService
{
    private readonly IDistributedCache _cache;
    private readonly IJsonSerializerService _serializer;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheVersionService _version;
    private readonly IAuthService _authService;
    private readonly ICacheBypassService _cacheBypassService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache used to store serialized responses.</param>
    /// <param name="logger">The cache behavior logger.</param>
    /// <param name="serializer">The JSON serializer used for cache payloads.</param>
    /// <param name="cacheOption">The configured cache lifetime options.</param>
    /// <param name="version">The scoped cache version service.</param>
    /// <param name="authService">The authenticated-principal accessor used for current-user scoped caches.</param>
    /// <param name="cacheBypassService">The registry used to skip cache scopes with uncertain invalidation state.</param>
    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        IJsonSerializerService serializer,
        IOptions<CacheOptions> cacheOption,
        ICacheVersionService version,
        IAuthService authService,
        ICacheBypassService cacheBypassService)
    {
        _cache = cache;
        _logger = logger;
        _serializer = serializer;
        _cacheOptions = cacheOption.Value;
        _version = version;
        _authService = authService;
        _cacheBypassService = cacheBypassService;
    }

    /// <summary>
    /// Handles the caching workflow:
    /// 1) If <paramref name="message"/> requests bypass, execute handler and return.
    /// 2) Build a deterministic cache key based on request params + epoch + version.
    /// 3) Try cache hit -> return cached response.
    /// 4) Cache miss -> execute handler, store response to cache, return response.
    /// </summary>
    /// <param name="message">
    /// Represents the incoming request for which caching behavior is being applied. The request may
    /// include cache-control metadata such as an explicit expiration requested by the caller.
    /// </param>
    /// <param name="next">
    /// A delegate to the next handler in the pipeline. Invoked when a cache miss occurs or when a cache is explicitly bypassed.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that the operation should be canceled.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation, returning the response of type <typeparamref name="TResponse"/> after
    /// applying the caching behavior.
    /// </returns>
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Allow callers to explicitly bypass cache for troubleshooting or forced fresh reads.
        if (message.BypassCache)
        {
            _logger.LogDebug(CacheLogs.LOG_CACHE_BYPASSED, typeof(TRequest).Name);
            return await next(message, cancellationToken);
        }

        // Epoch partitions versions by time window (e.g., day) to avoid unbounded counters.
        // Version changes (bumped by command pipeline) ensure we never serve stale query results.
        var cacheScope = ResolveCacheScope(message);
        if (await _cacheBypassService.ShouldBypassAsync(message.CacheKey, cacheScope, cancellationToken))
        {
            _logger.LogWarning(CacheLogs.CACHE_SCOPE_BYPASSED_AFTER_INVALIDATION_FAILURE, message.CacheKey, cacheScope);
            return await next(message, cancellationToken);
        }

        var epoch = _version.GetEpoch();
        long ver;
        try
        {
            ver = await _version.GetAsync(message.CacheKey, cacheScope, epoch);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, CacheLogs.CACHE_VERSION_READ_BYPASSED, typeof(TRequest).Name, message.CacheKey, cacheScope);
            return await next(message, cancellationToken);
        }

        var key = CacheKeyHelper.BuildKey(message, cacheScope, epoch, ver);

        // 1) Cache read
        byte[] cached;
        try
        {
            cached = await _cache.GetAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, CacheLogs.CACHE_READ_BYPASSED, typeof(TRequest).Name, key);
            return await next(message, cancellationToken);
        }

        if (cached is not null)
        {
            _logger.LogInformation(
                FETCHED_FROM_CACHE,
                key);

            return _serializer.Deserialize<TResponse>(Encoding.UTF8.GetString(cached));
        }

        // 2) Cache miss -> execute handler (DB / external calls)
        _logger.LogDebug(
            CacheLogs.LOG_CACHE_MISS,
            typeof(TRequest).Name, key, epoch, ver);

        var response = await next(message, cancellationToken);

        // 3) Cache write
        // FE/request-provided TTL is allowed; otherwise cache data uses CacheSettings:AbsoluteExpiration.
        var ttl = message.AbsoluteExpiration is { } requestedTtl && requestedTtl > TimeSpan.Zero
            ? requestedTtl
            : TimeSpan.FromSeconds(_cacheOptions.AbsoluteExpiration);

        try
        {
            await _cache.SetAsync(
                key,
                Encoding.UTF8.GetBytes(_serializer.Serialize(response)),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                cancellationToken);

            _logger.LogInformation(
                ADDED_TO_CACHE,
                key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, CacheLogs.CACHE_WRITE_SKIPPED, typeof(TRequest).Name, key);
        }

        return response;
    }

    /// <summary>
    /// Resolves the effective cache scope for the current request from explicit request metadata or the authenticated principal.
    /// </summary>
    /// <param name="request">The cacheable request whose scope is being resolved.</param>
    /// <returns>The effective cache scope string, or an empty string when the request is unscoped.</returns>
    private string ResolveCacheScope(TRequest request)
    {
        if (string.Equals(request.CacheScope, CURRENT_USER_CACHE_SCOPE, StringComparison.Ordinal))
        {
            // Current-user scoped queries keep the cache contract local to the request while delaying
            // the concrete user namespace resolution until the authenticated principal is available.
            var currentUserPublicId = _authService.UserId();
            if (!currentUserPublicId.HasValue)
            {
                _logger.LogDebug(CacheLogs.LOG_CACHE_CURRENT_USER_SCOPE_SKIPPED, typeof(TRequest).Name);
                return string.Empty;
            }

            _logger.LogDebug(CacheLogs.LOG_CACHE_CURRENT_USER_SCOPE_RESOLVED, typeof(TRequest).Name);

            return currentUserPublicId.Value.ToUserCacheScope();
        }

        return request.CacheScope ?? string.Empty;
    }
}
