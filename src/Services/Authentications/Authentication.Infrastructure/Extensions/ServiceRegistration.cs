namespace Authentication.Infrastructure.Extensions;

public static class ServiceRegistration
{
    /// <summary>
    /// Registers the infrastructure layer services and dependencies to the specified service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the services will be added.</param>
    /// <param name="configuration">The IConfiguration instance containing configuration values.</param>
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbConnectionInitialization();
        services.AddConfiguredDbContext<AuthenticationDbContext>();
        services.AddUnitOfWork<AuthenticationDbContext>();
        services.AddEmailService(configuration);
        services.Configure<ExternalAuthenticationOptions>(configuration.GetSection("ExternalAuthenticationSettings"));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IExternalLoginRepository, ExternalLoginRepository>();
        services.AddScoped<IMasterDataValueRepository, MasterDataValueRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPartyRepository, PartyRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }
}
