namespace Haven.Infrastructure.Extensions;

/// <summary>
/// Registers Haven business infrastructure dependencies.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers Haven infrastructure services and dependencies with the supplied service collection.
    /// </summary>
    /// <param name="services">The service collection receiving Haven infrastructure registrations.</param>
    public static void AddInfrastructure(this IServiceCollection services)
    {
        // Register shared relational infrastructure before services and repositories consume it.
        services.AddDbConnectionInitialization();
        services.AddDapperInfrastructure();
        services.AddConfiguredDbContext<HavenDbContext>();
        services.AddUnitOfWork<HavenDbContext>();

        // Register use-case services that coordinate landlord-scoped business workflows.
        services.AddScoped<IPartyService, PartyService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IRoomPackageService, RoomPackageService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IRoomMeterService, RoomMeterService>();

        // Register entity-specific repositories for read projections and tracked mutation graphs.
        services.AddScoped<IPartyRepository, PartyRepository>();
        services.AddScoped<IMasterDataRepository, MasterDataRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IUnitPackageRepository, UnitPackageRepository>();
        services.AddScoped<IUnitPackageItemRepository, UnitPackageItemRepository>();
        services.AddScoped<IPropertyPartyRepository, PropertyPartyRepository>();
        services.AddScoped<IRentalChargePolicyRepository, RentalChargePolicyRepository>();
        services.AddScoped<PropertyRepositoryDependencies>();
        services.AddScoped<IMeterRepository, MeterRepository>();
        services.AddScoped<IInvoiceUtilityRepository, InvoiceUtilityRepository>();
        services.AddScoped<MeterRepositoryDependencies>();
    }
}
