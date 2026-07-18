namespace Be.Haven.Cache.Extensions;

/// <summary>
/// Registers cache, login-attempt, cache-version, bypass, and distributed-lock infrastructure.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers in-memory cache services or one shared Redis connection for cache and lock operations.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="configuration">The application configuration containing cache settings.</param>
    public static void AddDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Step 1: Bind cache options before selecting the backing provider.
        services.Configure<CacheOptions>(configuration.GetSection(CACHE_SETTING));
        services.Configure<LoginAttemptOptions>(configuration.GetSection(LOGIN_AT_TEMPT_SETTINGS));

        // Step 2: Resolve the cache mode once so all related services use the same infrastructure boundary.
        var cacheOptions = configuration.GetSection(CACHE_SETTING).Get<CacheOptions>()
            ?? throw new InvalidOperationException(ERROR_CACHE_OPTION_MISSING);

        if (cacheOptions.IsMemory)
        {
            // Step 3a: Register only local implementations when the application explicitly selects memory mode.
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICachingService, CachingService>();
            services.AddSingleton<IAtomicCacheService, InMemoryAtomicCacheService>();
            services.AddSingleton<ICacheBypassService, InMemoryCacheBypassService>();
            services.AddTransient<ILoginAttemptService, LoginMemoryAttemptService>();
            services.AddTransient<ICacheVersionService, InMemoryCacheVersionService>();
            return;
        }

        // Step 3b: Validate the Redis endpoint before registering any Redis-backed service.
        var redisConfiguration = BuildRedisConfiguration(cacheOptions.RedisSettings);

        // Step 4: Create one multiplexer lazily so registration itself does not open a network connection.
        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(
                typeof(ServiceRegistration).FullName ?? nameof(ServiceRegistration));

            try
            {
                var connection = ConnectionMultiplexer.Connect(redisConfiguration);

                if (!connection.IsConnected)
                {
                    logger.LogWarning(DistributedLockLogs.REDIS_CONNECTION_NOT_READY);
                }

                return connection;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    DistributedLockLogs.REDIS_INITIALIZATION_FAILED);

                throw new InvalidOperationException(ERROR_DISTRIBUTED_CACHE, exception);
            }
        });

        // Step 5: Reuse the multiplexer for cache payloads, login attempts, and distributed locks.
        services.AddStackExchangeRedisCache(_ => { });
        services.AddOptions<RedisCacheOptions>()
            .Configure<IConnectionMultiplexer>((options, connection) =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connection);
            });

        services.AddSingleton<IDistributedLockService, DistributedLockService>();
        services.AddSingleton<ICachingService, CachingService>();
        services.AddSingleton<IAtomicCacheService, AtomicCacheService>();
        services.AddSingleton<ICacheVersionService, DistributedCacheVersionService>();
        services.AddSingleton<ICacheBypassService, DistributedCacheBypassService>();
        services.AddTransient<ILoginAttemptService, LoginAttemptService>();
    }

    /// <summary>
    /// Builds and validates the Redis client configuration used by all Redis-backed services.
    /// </summary>
    /// <param name="redisOptions">The configured Redis endpoint and client settings.</param>
    /// <returns>The validated StackExchange.Redis configuration.</returns>
    private static ConfigurationOptions BuildRedisConfiguration(RedisOptions redisOptions)
    {
        ArgumentNullException.ThrowIfNull(redisOptions);

        if (string.IsNullOrWhiteSpace(redisOptions.Host))
        {
            throw new InvalidOperationException(ERROR_REDIS_HOST_MISSING);
        }

        if (redisOptions.Port <= 0)
        {
            throw new InvalidOperationException(ERROR_REDIS_PORT_INVALID);
        }

        var result = new ConfigurationOptions
        {
            Ssl = redisOptions.Ssl,
            Password = redisOptions.Password,
            DefaultDatabase = redisOptions.DbNumber,
            AbortOnConnectFail = false,
            ConnectRetry = redisOptions.ConnectRetry,
            ConnectTimeout = redisOptions.ConnectTimeout,
            SyncTimeout = redisOptions.SyncTimeout
        };

        result.EndPoints.Add(redisOptions.Host, redisOptions.Port);
        return result;
    }
}
