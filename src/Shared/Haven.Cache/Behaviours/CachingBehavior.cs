namespace Haven.Cache.Behaviours;

/// <summary>
/// MediatR pipeline behavior that caches responses for cacheable query requests.
///
/// Key points:
/// - Only applies to requests implementing <see cref="ICacheableMediatrQueryService"/> (queries).
/// - Uses an epoch + version strategy to avoid stale data after writes:
///   CacheKey := "{GroupCacheKey}:e{epoch}:v{version}:{paramsHash}"
/// - The version is expected to be bumped by a separate behavior after successful commands,
///   so subsequent queries automatically read/write a new cache key and never return stale data.
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICacheableMediatrQueryService
{
    private readonly IDistributedCache _cache;
    private readonly IJsonSerializerService _serializer;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheVersionService _version;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        IJsonSerializerService serializer,
        IOptions<CacheOptions> cacheOption,
        ICacheVersionService version)
    {
        _cache = cache;
        _logger = logger;
        _serializer = serializer;
        _cacheOptions = cacheOption.Value;
        _version = version;
    }

    /// <summary>
    /// Handles the caching workflow:
    /// 1) If <paramref name="request"/> requests bypass, execute handler and return.
    /// 2) Build a deterministic cache key based on request params + epoch + version.
    /// 3) Try cache hit -> return cached response.
    /// 4) Cache miss -> execute handler, store response to cache, return response.
    /// </summary>
    /// <param name="request">
    /// Represents the incoming request for which caching behavior is being applied. The request may
    /// include specific caching parameters such as whether to bypass the cache or define expiration settings.
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
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Allow callers to explicitly bypass cache for troubleshooting or forced fresh reads.
        if (request.BypassCache)
        {
            _logger.LogDebug(CacheLogs.LOG_CACHE_BYPASSED, typeof(TRequest).Name);
            return await next(cancellationToken);
        }

        // Epoch partitions versions by time window (e.g., day) to avoid unbounded counters.
        // Version changes (bumped by command pipeline) ensure we never serve stale query results.
        var epoch = _version.GetEpoch();
        var ver = await _version.GetAsync(epoch);

        var key = BuildKey(request, epoch, ver);

        // 1) Cache read
        var cached = await _cache.GetAsync(key, cancellationToken);
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

        var response = await next(cancellationToken);

        // 3) Cache write
        // Sliding expiration keeps hot keys alive while they are accessed frequently.
        // (If you want more predictability in large systems, consider AbsoluteExpirationRelativeToNow.)
        var ttl = request.AbsoluteExpiration
                  ?? TimeSpan.FromSeconds(_cacheOptions.AbsoluteExpiration);

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

        return response;
    }

    /// <summary>
    /// Builds a deterministic cache key for the given request.
    ///
    /// Format:
    /// "{request.CacheKey}:e{epoch}:v{ver}:{paramsHash}"
    ///
    /// - request.CacheKey is the logical "group" (e.g., "invoice:list:factoryA:2026").
    /// - epoch groups counters by time window (e.g., daily).
    /// - ver is an incrementing version for invalidation (bumped after successful commands).
    /// - paramsHash is a stable hash of request parameters (sorted key=value pairs).
    /// </summary>
    /// <param name="request">The request object that implements <see cref="ICacheableMediatrQueryService"/>
    /// and contains the parameters for the cache key.</param>
    /// <param name="epoch">A string representing the epoch value, used to partition the cache by time window.</param>
    /// <param name="ver">A numeric value representing the version, used to ensure consistency and avoid serving stale data.</param>
    /// <returns>A hashed and formatted string that uniquely identifies the cache entry for the provided request.</returns>
    private static string BuildKey(TRequest request, string epoch, long ver)
    {
        var dictionary = request.AsDictionary();

        // Build a stable, deterministic "key=value|key=value" string:
        // - Excludes cache-control parameters.
        // - Sorts by key to avoid nondeterminism due to dictionary ordering.
        var raw = string.Join(PIPE_SEPARATOR,
            dictionary.Where(x => x.Value is not null
                         && x.Key is not CACHEKEY
                         && x.Key is not BYPASSCACHE
                         && x.Key is not ABSOLUTEEXPIRATION)
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Value}")
        );

        // Hash the parameter string to:
        // - keep Redis keys short,
        // - reduce PII exposure in key names,
        // - avoid very long keys for large requests.
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)))
                  .ToLowerInvariant()[..32]; // 16 bytes hex -> short and sufficient for cache keys

        return $"{request.CacheKey}:e{epoch}:v{ver}:{hash}";
    }
}