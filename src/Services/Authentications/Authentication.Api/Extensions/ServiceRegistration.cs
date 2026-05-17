namespace Authentication.Api.Extensions;

/// <summary>
/// Registers API-layer configuration and authentication services for the authentication module.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Configures and registers application options by binding them to sections in the application configuration.
    /// Validates the options' data annotations and ensures validation occurs at startup.
    /// </summary>
    /// <param name="services">The service collection to which the options will be added.</param>
    /// <param name="configuration">The application configuration containing the sections to bind to options.</param>
    public static void AddConfiguredOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Token validation options bind after startup secret-overlay resolution has produced final values.
        services.AddHavenTokenValidationOptions(configuration);
        services.AddConfiguredOption<GcpOptions>(configuration, GCP_SETTINGS, validateDataAnnotations: true);
        services.AddAuthOptions(configuration);
        services.AddConfiguredOption<ExternalAuthenticationOptions>(configuration, EXTERNAL_AUTHENTICATION_SETTINGS);
    }

    /// <summary>
    /// Binds authentication token issuing options after startup secret-overlay resolution.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    private static void AddAuthOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Token issuing uses the same final secret value as the shared token validator.
        services.AddOptions<AuthOptions>()
                .Bind(configuration.GetSection(AUTH_SETTINGS))
                .ValidateOnStart();
    }

    /// <summary>
    /// Configures JWT bearer authentication for the authentication API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddAuthServices(this IServiceCollection services)
    {
        // Authentication.Api uses the shared Haven bearer-token handler from ApiCommon.
        services.AddHavenAuthenticationServices();
    }

    /// <summary>
    /// Registers the source-generated mediator for authentication application handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddMediatorServices(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(global::Authentication.Application.Extensions.ServiceRegistration)];
        });
    }

    /// <summary>
    /// Binds and validates a strongly typed options object from configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to bind.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <param name="validateDataAnnotations">Indicates whether data-annotation validation should be applied.</param>
    private static void AddConfiguredOption<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        bool validateDataAnnotations = false)
        where TOptions : class
    {
        var optionsBuilder = services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName));

        if (validateDataAnnotations)
        {
            optionsBuilder.ValidateDataAnnotations();
        }

        optionsBuilder.ValidateOnStart();
    }
}
