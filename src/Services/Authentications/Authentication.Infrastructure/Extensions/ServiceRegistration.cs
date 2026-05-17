namespace Authentication.Infrastructure.Extensions;

/// <summary>
/// Registers infrastructure services, repositories, and persistence dependencies for the authentication module.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers the infrastructure layer services and dependencies to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which infrastructure services will be added.</param>
    /// <param name="configuration">The application configuration used by infrastructure dependencies.</param>
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbConnectionInitialization();
        services.AddDapperInfrastructure();
        services.AddConfiguredDbContext<AuthenticationDbContext>();
        services.AddUnitOfWork<AuthenticationDbContext>();
        services.AddEmailService(configuration);
        services.AddAuthenticatedHttpClientMultiple<IRestClientMultipleService, RestClientMultipleService>(
            (AppConstants.SystemVariable.UPLOAD_R2_OBJECT, null, null));
        services.AddScoped<IThirdPartyApiService, ThirdPartyApiService>();
        services.AddR2ObjectStorageService(configuration);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IExternalLoginRepository, ExternalLoginRepository>();
        services.AddScoped<IMasterDataValueRepository, MasterDataValueRepository>();
        services.AddScoped<IPartyRepository, PartyRepository>();
        services.AddScoped<IPartyVehicleRepository, PartyVehicleRepository>();
        services.AddScoped<IUserPartyRepository, UserPartyRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentLinkRepository, DocumentLinkRepository>();
        services.AddScoped<IPartyIdentifierRepository, PartyIdentifierRepository>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<AuthenticationRepositoryDependencies>();
        services.AddScoped<AuthenticationKycRepositoryDependencies>();
        services.AddScoped<AuthenticationServiceSupportDependencies>();
        services.AddScoped<UserServiceSupportDependencies>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IExternalAuthenticationService, ExternalAuthenticationService>();
        services.AddScoped<IUserService, UserService>();
    }
}
