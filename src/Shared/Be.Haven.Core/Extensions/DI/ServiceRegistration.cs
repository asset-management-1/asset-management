namespace Be.Haven.Core.Extensions.DI;

public static class ServiceRegistration
{
    /// <summary>
    /// Adds shared infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddCoreInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        services.AddSingleton<IJsonSerializerService, JsonSerializerService>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientDeviceContextAccessor, ClientDeviceContextAccessor>();
        
        // Register Mapster configurations for object mapping across layers.
        RegisterMapperConfigurations();
    }

    /// <summary>
    /// Adds the Unit of Work pattern implementation to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context to use within the Unit of Work.</typeparam>
    /// <param name="services">The service collection to which the Unit of Work services are added.</param>
    public static void AddUnitOfWork<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
    }
    
    /// <summary>
    /// Adds an authenticated HttpClient without JWT to the service collection.
    /// </summary>
    /// <typeparam name="TInterface">The interface type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">Optional client configuration action.</param>
    /// <returns>The updated service collection.</returns>
    public static void AddAuthenticatedHttpClient<TInterface, TImplementation>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient = null)
        where TImplementation : class, TInterface
        where TInterface : class
    {
        // Register the named HttpClient
        services.AddHttpClient<TInterface, TImplementation>((provider, client) =>
                {
                    configureClient?.Invoke(provider, client);
                })
                .AddHttpMessageHandler<CorrelationIdHandler>()
                .AddTransientHttpErrorPolicy(policy =>
                    policy.CircuitBreakerAsync(
                        RETRY_NUMBER_CIRCUIT, // Number of retries before breaking the circuit
                        TimeSpan.FromMinutes(DURATION_OF_BREAK))); // Duration of the circuit break
    }

    /// <summary>
    /// Adds an authenticated HttpClient with a custom token handler to the service collection.
    /// </summary>
    /// <typeparam name="TInterface">The interface type for the HttpClient.</typeparam>
    /// <typeparam name="TImplementation">The implementation type for the HttpClient.</typeparam>
    /// <typeparam name="TTokenHandler">The custom token handler type for authentication.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">Optional client configuration action.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAuthenticatedHttpClient<TInterface, TImplementation, TTokenHandler>(
        this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient = null)
        where TInterface : class
        where TImplementation : class, TInterface
        where TTokenHandler : DelegatingHandler
    {
        // Register the HttpClient with the specified interface, implementation, and token handler
        services.AddHttpClient<TInterface, TImplementation>((provider, client) =>
                {
                    configureClient?.Invoke(provider, client);
                })
                .AddHttpMessageHandler<CorrelationIdHandler>()
                .AddHttpMessageHandler<TTokenHandler>() // Add the custom token handler for authentication
                .AddTransientHttpErrorPolicy(policy =>
                    policy.CircuitBreakerAsync(
                        RETRY_NUMBER_CIRCUIT, // Number of retries before breaking the circuit
                        TimeSpan.FromMinutes(DURATION_OF_BREAK))); // Duration of the circuit break

        return services;
    }

    /// <summary>
    /// Registers an authenticated HTTP client with dynamic configuration options for services and their implementations.
    /// </summary>
    /// <param name="services">The service collection to which the authenticated HTTP client is added.</param>
    /// <param name="clientName">The name of the HTTP client. Defaults to 'default-client'.</param>
    /// <param name="configureClient">An optional delegate for configuring the HttpClient, such as BaseAddress, Timeout, or default headers.</param>
    /// <param name="configureBuilder">An optional delegate for configuring the HttpClientBuilder, such as handlers or policies.</param>
    /// <typeparam name="TService">The interface type of the service to be injected.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the service to be injected.</typeparam>
    public static void AddAuthenticatedHttpClientMultiple<TService, TImplementation>(
        this IServiceCollection services,
        string clientName = DEFAULT_CLIENT,
        Action<IServiceProvider, HttpClient> configureClient = null,
        Action<IHttpClientBuilder> configureBuilder = null)
        where TService : class
        where TImplementation : class, TService
    {
        var builder = services.AddHttpClient(clientName, (sp, http) =>
        {
            // Optional per-client HttpClient config (BaseAddress, Timeout, default headers, …)
            configureClient?.Invoke(sp, http);
        }).AddHttpMessageHandler<CorrelationIdHandler>();

        // Optional per-client builder config (handlers, primary handler, …)
        configureBuilder?.Invoke(builder);

        // Add a circuit breaker policy to handle transient faults
        builder.AddTransientHttpErrorPolicy(p =>
            p.CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: RETRY_NUMBER_CIRCUIT,
                durationOfBreak: TimeSpan.FromMinutes(DURATION_OF_BREAK)));

        services.AddTransient<TService, TImplementation>();
    }

    /// <summary>
    /// Adds dynamically configured HTTP clients to the service collection
    /// with support for authentication and custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="clients">An array of tuples defining the name, client configuration,
    /// and client builder configuration for each HTTP client.</param>
    /// <typeparam name="TService">The interface type of the service.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    public static void AddAuthenticatedHttpClientMultiple<TService, TImplementation>(
        this IServiceCollection services,
        params (string Name,
            Action<IServiceProvider, HttpClient> ConfigureClient,
            Action<IHttpClientBuilder> ConfigureBuilder)[] clients)
        where TService : class
        where TImplementation : class, TService
    {
        foreach (var (name, configureClient, configureBuilder) in clients)
        {
            var builder = services.AddHttpClient(name, (sp, http) =>
            {
                // Optional per-client HttpClient config (BaseAddress, Timeout, default headers, …)
                configureClient?.Invoke(sp, http);
            }).AddHttpMessageHandler<CorrelationIdHandler>();

            // Optional per-client builder config (handlers, primary handler, …)
            configureBuilder?.Invoke(builder);

            // Add a circuit breaker policy to handle transient faults
            builder.AddTransientHttpErrorPolicy(p =>
                p.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: RETRY_NUMBER_CIRCUIT,
                    durationOfBreak: TimeSpan.FromMinutes(DURATION_OF_BREAK)));
        }

        services.AddTransient<TService, TImplementation>();
    }
    
    /// <summary>
    /// Registers Google Pub/Sub publisher-related services into the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing Google Pub/Sub settings.</param>
    public static void AddGooglePubSubPublisher(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<QueueInfoOptions>(configuration.GetSection(QUEUE_INFO_SETTINGS));
        serviceCollection.AddSingleton(sp => sp.GetRequiredService<IOptions<QueueInfoOptions>>().Value);
        serviceCollection.AddSingleton<IGcpPubSubPublisherClientFactory, GcpPubSubPublisherClientFactoryService>();
        serviceCollection.AddSingleton<IGcpPublisherService, GcpPublisherService>();
    }

    /// <summary>
    /// Registers the Google Pub/Sub Subscriber in the service collection and allows custom configuration
    /// for the subscriber job. This method is typically used to set up processing of messages
    /// from a Pub/Sub subscription as a background task.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the subscriber to.</param>
    /// <param name="action">A delegate to configure the behaviour of the GcpSubscriberJob instance.</param>
    public static void AddGooglePubSubSubscriber(
        this IServiceCollection serviceCollection,
        Action<GcpSubscriberJob> action)
    {
        // Registers the hosted service using the DI container and allows custom configuration
        serviceCollection.AddHostedService(sp =>
        {
            // Resolve the logger for the subscriber job from the DI container
            var logger = sp.GetRequiredService<ILogger<GcpSubscriberJob>>();

            // Create an instance of the subscriber job with its dependencies injected
            var job = ActivatorUtilities.CreateInstance<GcpSubscriberJob>(sp, logger);

            // Apply custom handler configuration provided by the caller
            action(job);

            // Return the configured job to be run as a hosted background service
            return job;
        });
    }

    /// <summary>
    /// Configures global mappings using the Mapster library for the current assembly.
    /// Applies settings for reference preservation, deep copy, and disables constructor mappings.
    /// Scans the loaded assemblies for mapping configurations and automatically registers them.
    /// </summary>
    private static void RegisterMapperConfigurations()
    {
        // Get the assembly of the Core layer
        var assembly = AppDomain.CurrentDomain.GetAssemblies();

        TypeAdapterConfig.GlobalSettings.Default
                         .PreserveReference(true) // Enables support for circular references to prevent stack overflow.
                         .ShallowCopyForSameType(
                             false) // Enables deep copy for objects of the same type, ensuring no shared references.
                         .MapToConstructor(
                             false); // Disables mapping through the constructor; properties are mapped directly.

        // Scan and automatically apply mapping configurations 
        // defined throughout the scanned assembly (e.g., profiles implementing IRegister).
        TypeAdapterConfig.GlobalSettings.Scan(assembly);
    }

    /// <summary>
    /// Adds Dapper infrastructure, including database connection factory and Dapper service, to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddDapperInfrastructure(this IServiceCollection services)
    {
        // IDbConnectionFactory is stateless; register it as a singleton to reuse the same lightweight factory instance.
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        // IDapperService opens and disposes DbConnection per operation.
        // Scoped lifetime aligns with the ASP.NET Core request scope and keeps flexibility
        // if you later inject scoped dependencies (e.g., logging scopes, correlation, etc.).
        services.AddScoped<IDapperService, DapperService>();
    }
    
    /// <summary>
    /// Adds services for initializing database connections to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which the services are added.</param>
    public static void AddDbConnectionInitialization(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
        services.AddHostedService<DbConnectionHostService>();
    }
    
    /// <summary>
    /// Registers a DbContext with a specific connection string and environment configurations.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext to register.</typeparam>
    /// <param name="services">The service collection for DI.</param>
    /// <param name="connectionName">The name of the connection string in the configuration.</param>
    /// <param name="configureOptions">An optional additional configuration for DbContextOptions.</param>
    public static void AddConfiguredDbContext<TContext>(
        this IServiceCollection services,
        string connectionName = DEFAULT_CONNECTION,
        Action<DbContextOptionsBuilder> configureOptions = null)
        where TContext : DbContext
    {
        // Register the DbContext using the connection string and allow additional customization through Action
        services.AddDbContext<TContext>((sp, options) =>
        {
            var csProvider = sp.GetRequiredService<IConnectionStringProvider>();
            var gcpOptions = sp.GetRequiredService<IOptions<GcpOptions>>().Value;
            var databaseSettings = gcpOptions.DatabaseSettings ?? new DatabaseOptions();
            var resolvedConnectionName = string.IsNullOrWhiteSpace(databaseSettings.ConnectionName)
                ? connectionName
                : databaseSettings.ConnectionName;
            var resolvedProvider = databaseSettings.Provider;
            var connectionString = csProvider.GetConnectionString(resolvedConnectionName);

            // Use the configured provider once here so every DbContext follows the same runtime database.
            switch (resolvedProvider)
            {
                case SqlProvider.PostgreSql:
                    options.UseNpgsql(
                        connectionString,
                        postgresOptions => postgresOptions
                            .UseNetTopologySuite()
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                            .CommandTimeout(SQL_COMMAND_TIMEOUT_SECONDS)
                            .EnableRetryOnFailure(
                                SQL_RETRY_COUNT,
                                TimeSpan.FromSeconds(SQL_RETRY_DELAY_SECONDS),
                                null));
                    break;
                case SqlProvider.SqlServer:
                    options.UseSqlServer(
                        connectionString,
                        sqlOptions => sqlOptions
                            .UseNetTopologySuite()
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                            .CommandTimeout(SQL_COMMAND_TIMEOUT_SECONDS)
                            .EnableRetryOnFailure(
                                SQL_RETRY_COUNT,
                                TimeSpan.FromSeconds(SQL_RETRY_DELAY_SECONDS),
                                null));
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format(ERROR_UNSUPPORTED_DATABASE_PROVIDER, resolvedProvider));
            }
            
            options.AddInterceptors(new DbConnectionRefreshInterceptor(
                csProvider,
                sp.GetRequiredService<ILogger<DbConnectionRefreshInterceptor>>(),
                resolvedConnectionName));
            
            if (Environment.GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT) == ENVIRONMENT_DEVELOPMENT)
            {
                options.EnableSensitiveDataLogging();
            }

            configureOptions?.Invoke(options);
        });
    }

    /// <summary>
    /// Adds the Google Cloud Platform (GCP) Secret Manager infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which GCP Secret Manager services will be added.</param>
    /// <param name="configuration">The configuration object containing GCP settings.</param>
    public static void AddGcpSecretManagerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind all GCP-related options at once here. The service itself does not read IConfiguration.
        var opts = configuration.GetSection(GCP_SETTINGS).Get<GcpOptions>() ?? new GcpOptions();

        if (opts.SecretManagerSettings.IsUseSecret)
        {
            // Register the Secret Manager client only when GCP secrets are enabled.
            services.AddSingleton(_ => SecretManagerResourceHelper.BuildClient(opts.SecretManagerSettings.Location));
        }

        // Register the runtime secret service; local mode returns configured values without calling GCP.
        services.AddSingleton<IGcpSecretService>(sp =>
        {
            // Local configuration mode never calls the GCP client, so avoid requiring ADC at startup.
            var client = opts.SecretManagerSettings.IsUseSecret
                ? sp.GetRequiredService<SecretManagerServiceClient>()
                : null;
            var logger = sp.GetRequiredService<ILogger<GcpSecretService>>();
            var cache = sp.GetRequiredService<ICachingService>();
            var gcpOptions = sp.GetRequiredService<IOptions<GcpOptions>>();

            // Construct the global or regional resource prefix for accessing secrets.
            var resourcePrefix = SecretManagerResourceHelper.BuildResourcePrefix(
                opts.ProjectNumber,
                opts.SecretManagerSettings.Location);

            return new GcpSecretService(
                client,
                logger,
                cache,
                gcpOptions,
                resourcePrefix);
        });
    }

    /// <summary>
    /// Adds Google Cloud Storage client and upload service to the service collection.
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for chaining registrations.
    /// </returns>
    /// <example>
    /// Example usage:
    /// <code>
    /// services.AddGcpCloudFileProvider();
    /// </code>
    /// </example>
    public static IServiceCollection AddGcpCloudFileProvider(this IServiceCollection services)
    {
        // Register a singleton StorageClient used to communicate with Google Cloud Storage.
        // StorageClient handles upload, download, and delete operations.
        services.AddSingleton(_ => StorageClient.Create());

        // Register the GCP upload service as a singleton.
        // GcpUploadService uses StorageClient and ILogger to perform file operations and logging.
        services.AddSingleton<IGcpUploadService>(provider =>
            new GcpUploadService(
                provider.GetRequiredService<StorageClient>(),
                provider.GetRequiredService<ILogger<GcpUploadService>>())
        );

        // Return the IServiceCollection to allow chaining of other registrations.
        return services;
    }

    /// <summary>
    /// Registers the <see cref="IEmailService"/> and its dependencies in the dependency injection container.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which the email services are added.</param>
    /// <param name="configuration">The application configuration containing email-related settings.</param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance for method chaining.
    /// </returns>
    public static IServiceCollection AddEmailService(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        // Bind EmailOptions after startup secret-overlay resolution has produced final option values.
        serviceCollection.AddOptions<EmailOptions>()
                         .Bind(configuration.GetSection(EMAIL_SETTINGS));

        // Register EmailService as a singleton implementation of IEmailService
        serviceCollection.AddScoped<IEmailService, EmailService>();

        return serviceCollection;
    }

    /// <summary>
    /// Registers the Upload GCP service and its configuration into the dependency
    /// injection container.
    /// </summary>
    /// <param name="serviceCollection">
    /// The service collection to which the Upload GCP services will be added.
    /// </param>
    /// <param name="configuration">
    /// The application configuration used to bind <see cref="UploadGcpOptions"/>.
    /// </param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/> instance for chaining.
    /// </returns>
    public static IServiceCollection AddUploadGcpService(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        // Bind UploadGcpOptions from the configuration section "UploadGcpSettings"
        serviceCollection.Configure<UploadGcpOptions>(configuration.GetSection(UPLOAD_GCP_SETTINGS));

        // Register the GCP upload service for handling file upload operations
        serviceCollection.AddScoped<IUploadGcpService, UploadGcpService>();

        return serviceCollection;
    }

    /// <summary>
    /// Registers the shared object storage abstraction backed by Cloudflare R2.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which object storage services are added.</param>
    /// <param name="configuration">The application configuration used to bind <see cref="R2StorageOptions"/>.</param>
    /// <returns>The modified service collection for chaining.</returns>
    public static IServiceCollection AddR2ObjectStorageService(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddOptions<R2StorageOptions>()
                         .Bind(configuration.GetSection(R2_STORAGE_SETTINGS));
        serviceCollection.AddScoped<IObjectStorageService, R2ObjectStorageService>();

        return serviceCollection;
    }

    /// <summary>
    /// Registers Quartz and schedules jobs dynamically by scanning assemblies for classes implementing <see cref="IJob"/>
    /// decorated with <see cref="QuartzJobKeyAttribute"/>.
    /// 
    /// Scheduling is driven by configuration:
    /// - If a job key exists in QuartzJobsOptions.Jobs and Enabled = true, the job will be scheduled.
    /// - If CronExpression is provided, a cron trigger is used.
    /// - Otherwise, a simple trigger with IntervalSeconds is used (default fallback is 300 seconds).
    /// </summary>
    /// <param name="services">The service collection to which Quartz services will be added.</param>
    /// <param name="configuration">The application configuration used to bind Quartz settings.</param>
    /// <returns>The updated service collection with configured Quartz integration.</returns>
    public static void AddConfiguredQuartz(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind Quartz options once and validate at startup.
        services.AddOptions<QuartzJobsOptions>()
            .Bind(configuration.GetSection(QUARTZ_SETTINGS))
            .ValidateOnStart();

        services.AddQuartz(q =>
        {
            // Read configuration once at startup (Quartz schedules are created during startup).
            var opts = configuration.GetSection(QUARTZ_SETTINGS).Get<QuartzJobsOptions>()
                       ?? new QuartzJobsOptions();

            // Scan all loaded (non-dynamic) assemblies to discover Quartz jobs.
            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic);

            // Find all IJob implementations that have [QuartzJobKey("...")].
            var discoveredJobs =
                assemblies.SelectMany(SafeGetTypes)
                          .Where(t => !t.IsAbstract && typeof(IJob).IsAssignableFrom(t))
                          .Select(t => new { Type = t, Attr = t.GetCustomAttribute<QuartzJobKeyAttribute>() })
                          .Where(x => x.Attr is not null)
                          .Select(x => new { x.Type, x.Attr!.Key });

            // Prevent duplicate scheduling if the same job type/key is found multiple times.
            var scheduledKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var j in discoveredJobs)
            {
                // Skip duplicates by key.
                if (!scheduledKeys.Add(j.Key))
                {
                    continue;
                }

                // Only schedule jobs that exist in configuration and are enabled.
                if (!opts.Jobs.TryGetValue(j.Key, out var cfg) || !cfg.Enabled)
                {
                    continue;
                }

                // Quartz job identity = job key from attribute/config.
                var jobKey = new JobKey(j.Key);

                // Register the job by runtime Type (works with Quartz DI extensions).
                q.AddJob(j.Type, jobKey, jb => jb.WithIdentity(jobKey));

                // Register trigger for the job.
                q.AddTrigger(t =>
                {
                    t.WithIdentity($"{j.Key}-trigger")
                     .ForJob(jobKey);

                    // If CronExpression is defined, use a cron trigger.
                    if (!string.IsNullOrWhiteSpace(cfg.CronExpression))
                    {
                        t.WithCronSchedule(cfg.CronExpression);
                        return;
                    }

                    // Otherwise, use a simple interval trigger.
                    // Fallback to 300 seconds if IntervalSeconds is not specified.
                    var intervalSeconds = cfg.IntervalSeconds ?? 300;

                    t.StartNow()
                     .WithSimpleSchedule(x =>
                         x.WithIntervalInSeconds(intervalSeconds)
                          .RepeatForever());
                });
            }
        });

        // Hosted service that starts Quartz scheduler and waits for running jobs during shutdown.
        services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);
    }

    /// <summary>
    /// Gets loadable types from an assembly while preserving partial type-load results.
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>All loadable types from the assembly.</returns>
    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }
}
