namespace Haven.Api.Extensions;

/// <summary>
/// Registers Haven API authentication and mediator transport dependencies.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Configures bearer-token authentication for Haven business APIs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddAuthServices(this IServiceCollection services)
    {
        // Haven.Api shares the same token validation handler used by Authentication.Api.
        services.AddHavenAuthenticationServices();
    }

    /// <summary>
    /// Registers the source-generated mediator for Haven application handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddMediatorServices(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });
    }

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
        // Bind shared token-validation settings so the ApiCommon auth handler can validate Haven tokens.
        services.AddHavenTokenValidationOptions(configuration);

        // Reuse shared binding, annotation validation, and startup validation for Haven GCP settings.
        services.AddConfiguredOption<GcpOptions>(configuration, GCP_SETTINGS, validateDataAnnotations: true);
    }
}
