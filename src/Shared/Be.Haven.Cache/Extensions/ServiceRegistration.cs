namespace Be.Haven.Cache.Extensions;

public static class ServiceRegistration
{
    /// <summary>
    /// Configures and registers a distributed caching mechanism in the service collection
    /// based on the provided application configuration. Supports both in-memory and Redis caching.
    /// </summary>
    /// <param name="services">The service collection to which the caching services are added.</param>
    /// <param name="configuration">The application configuration interface for retrieving cache settings.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when there is an issue configuring or connecting to the distributed cache (e.g., Redis).
    /// </exception>
    public static void AddDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CacheOptions>(configuration.GetSection(CACHE_SETTING));
        services.Configure<LoginAttemptOptions>(configuration.GetSection(LOGIN_AT_TEMPT_SETTINGS));

        var cacheOption = configuration.GetSection(CACHE_SETTING).Get<CacheOptions>()
            ?? throw new InvalidOperationException(ERROR_CACHE_OPTION_MISSING);
        
        // Retrieve Redis configuration settings
        services.AddTransient<ICachingService, CachingService>(); // Add caching service to the service collection
        
        if (cacheOption.IsMemory)
        {
            services.AddDistributedMemoryCache(); // Add in-memory cache if the setting is enabled
            services.AddTransient<ILoginAttemptService, LoginMemoryAttemptService>();
            services.AddTransient<ICacheVersionService, InMemoryCacheVersionService>();
            return; // Exit the method
        }
 
        var passwordRedis = cacheOption.RedisSettings.Password;

        try
        {
            var configurationOptions = new ConfigurationOptions
            {
                Ssl = cacheOption.RedisSettings.Ssl, // Set SSL setting
                Password = passwordRedis, // Set Redis password
                DefaultDatabase = cacheOption.RedisSettings.DbNumber,
                AbortOnConnectFail = false,
                ConnectRetry = cacheOption.RedisSettings.ConnectRetry,
                ConnectTimeout = cacheOption.RedisSettings.ConnectTimeout,
                SyncTimeout = cacheOption.RedisSettings.SyncTimeout
            };
            configurationOptions.EndPoints.Add(cacheOption.RedisSettings.Host, cacheOption.RedisSettings.Port); // Set Redis host and port

            var connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
            if (!connectionMultiplexer.IsConnected)
            {
                Console.WriteLine(FAILED_TO_CONNECT_TO_REDIS);
            }

            // Add Redis connection multiplexer to the service collection
            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
            services.AddSingleton<IDistributedLockService, DistributedLockService>();
            services.AddSingleton<ICacheVersionService, DistributedCacheVersionService>();
            services.AddTransient<ILoginAttemptService, LoginAttemptService>();
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = configurationOptions; // Set Redis configuration options
            });
        }
        catch (Exception ex)
        {
            // Write an error log in case some issue related to add distributed cache
            Console.WriteLine($"{ERROR_DISTRIBUTED_CACHE}: {ex.Message}");

            throw new ArgumentException(ERROR_DISTRIBUTED_CACHE, ex.Message);
        }
    }

}
