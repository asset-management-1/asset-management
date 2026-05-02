namespace Authentication.Api.Extensions;

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

        services.AddOptions<AuthOptions>()
                .Bind(configuration.GetSection(AUTH_SETTINGS))
                .ValidateOnStart();
    }

    /// <summary>
    /// Configures JWT bearer authentication for the authentication API.
    /// </summary>
    public static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = configuration.GetSection(AUTH_SETTINGS).Get<AuthOptions>() ?? new AuthOptions();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey));

        services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = authOptions.Issuer,
                        ValidAudiences = authOptions.Audiences,
                        IssuerSigningKey = signingKey,
                        ClockSkew = TimeSpan.Zero
                    };
                });
    }
}
