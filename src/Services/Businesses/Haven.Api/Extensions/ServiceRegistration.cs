namespace Haven.Api.Extensions;

public static class ServiceRegistration
{
    /// <summary>
    /// Configures and registers application options by binding them to sections in the application configuration.
    /// Validates the options' data annotations and ensures validation occurs at startup.
    /// </summary>
    /// <param name="services">The service collection to which the options will be added.</param>
    /// <param name="configuration">The application configuration containing the sections to bind to options.</param>
    public static void AddConfiguredOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<GcpOptions>()
                .Bind(configuration.GetSection(GCP_SETTINGS))
                .ValidateDataAnnotations()
                .ValidateOnStart();
    }
}