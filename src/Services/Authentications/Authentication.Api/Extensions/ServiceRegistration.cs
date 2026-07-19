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
        services.AddConfiguredOption<AuthOptions>(
            configuration,
            AUTH_SETTINGS,
            validateDataAnnotations: true);
        services.AddConfiguredOption<ExternalAuthenticationOptions>(
                    configuration,
                    EXTERNAL_AUTHENTICATION_SETTINGS)
                .Validate(
                    ExternalProviderConfigurationIsValid,
                    ApiErrorConstants.EXTERNAL_PROVIDER_OPTIONS_INVALID);
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
    /// Determines whether the configured Google and Facebook provider contracts can fail fast safely.
    /// </summary>
    /// <param name="options">The external authentication options.</param>
    /// <returns><c>true</c> when both supported provider configurations are complete.</returns>
    private static bool ExternalProviderConfigurationIsValid(ExternalAuthenticationOptions options)
    {
        // Resolve both required mobile providers before evaluating their provider-specific startup contract.
        var google = options.Providers.FirstOrDefault(x => x.Name == ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE);
        var facebook = options.Providers.FirstOrDefault(x => x.Name == ApplicationConstants.EXTERNAL_PROVIDER_FACEBOOK);

        // Reject missing credentials, incomplete validation metadata, and unsupported provider entries at startup.
        return google is not null
               && (!string.IsNullOrWhiteSpace(google.Authority)
                   || !string.IsNullOrWhiteSpace(google.MetadataAddress))
               && google.Audiences.Count > 0
               && google.ValidIssuers.Count > 0
               && facebook is not null
               && !string.IsNullOrWhiteSpace(facebook.AppId)
               && !string.IsNullOrWhiteSpace(facebook.AppSecret)
               && !string.IsNullOrWhiteSpace(facebook.GraphApiBaseUrl)
               && !string.IsNullOrWhiteSpace(facebook.GraphApiVersion)
               && options.Providers.All(x => x.Name is ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE or ApplicationConstants.EXTERNAL_PROVIDER_FACEBOOK);
    }
}
