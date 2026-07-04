namespace Haven.Infrastructure.Extensions;

/// <summary>
/// Registers Haven business infrastructure dependencies.
/// </summary>
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
        services.AddDapperInfrastructure();
        services.AddConfiguredDbContext<HavenDbContext>();
        services.AddUnitOfWork<HavenDbContext>();
        services.AddScoped<IPartyService, PartyService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IPartyRepository, PartyRepository>();
        services.AddScoped<IMasterDataRepository, MasterDataRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IUnitPackageRepository, UnitPackageRepository>();
        services.AddScoped<IUnitPackageItemRepository, UnitPackageItemRepository>();
        services.AddScoped<IPropertyPartyRepository, PropertyPartyRepository>();
        services.AddScoped<IRentalChargePolicyRepository, RentalChargePolicyRepository>();
        services.AddScoped<PropertyRepositoryDependencies>();
    }
}
