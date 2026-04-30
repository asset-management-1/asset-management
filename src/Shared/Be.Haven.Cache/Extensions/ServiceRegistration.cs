using Be.Haven.Cache.Interfaces;
using Be.Haven.Cache.Services;

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
 
        var passwordRedis = GetRedisPasswordFromGcp(configuration, cacheOption);

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

    /// <summary>
    /// Retrieves the Redis password stored in Google Cloud Platform's Secret Manager
    /// based on the provided application configuration and cache options.
    /// </summary>
    /// <param name="configuration">The application configuration interface used to retrieve GCP settings.</param>
    /// <param name="cacheOption">The cache options containing Redis settings, including the secret ID of the Redis password.</param>
    /// <returns>The Redis password as a string retrieved securely from GCP's Secret Manager.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when any required Redis settings or GCP settings are missing, or if the retrieved password is empty.
    /// </exception>
    private static string GetRedisPasswordFromGcp(IConfiguration configuration, CacheOptions cacheOption)
    {
        if (string.IsNullOrWhiteSpace(cacheOption.RedisSettings.Password))
            return cacheOption.RedisSettings.Password;

        var gcpOpts = configuration.GetSection(GCP_SETTINGS).Get<GcpOptions>();

        // Build client (no DI, no cache)
        var client = new SecretManagerServiceClientBuilder
        {
            Endpoint = string.Format(SECRET_MANAGER_ENDPOINT_FORMAT, gcpOpts.SecretManagerSettings.Location),
        }.Build();

        var resourcePrefix = string.Format(
            SECRET_RESOURCE_PREFIX_FORMAT,
            gcpOpts.ProjectNumber,
            gcpOpts.SecretManagerSettings.Location);

        var ver = string.IsNullOrWhiteSpace(gcpOpts.SecretManagerSettings.DefaultSecretVersion)
            ? LATEST
            : gcpOpts.SecretManagerSettings.DefaultSecretVersion;

        var secretVersionName = $"{resourcePrefix}/{cacheOption.RedisSettings.Password}/{VERSIONS_SEGMENT}/{ver}";

        // Sync-over-async at startup (acceptable here); avoid .Result
        var resp = client.AccessSecretVersionAsync(secretVersionName)
                         .GetAwaiter().GetResult();

        var passwordRedis = resp.Payload.Data.ToStringUtf8();
        if (string.IsNullOrWhiteSpace(passwordRedis))
            throw new InvalidOperationException(REDIS_PASSWORD_SECRET_EMPTY);

        return passwordRedis;
    }
}