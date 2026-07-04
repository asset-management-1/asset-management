namespace Haven.Infrastructure.Context;

/// <summary>
/// Represents the Entity Framework context for the Haven business service.
/// </summary>
public partial class HavenDbContext : DbContext
{
    /// <summary>
    /// Creates the Haven business EF Core context.
    /// </summary>
    /// <param name="options">The configured database context options.</param>
    public HavenDbContext(DbContextOptions<HavenDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets property entities.
    /// </summary>
    public virtual DbSet<Property> Properties { get; set; }

    /// <summary>
    /// Gets or sets unit entities.
    /// </summary>
    public virtual DbSet<Unit> Units { get; set; }

    /// <summary>
    /// Gets or sets property-party link entities.
    /// </summary>
    public virtual DbSet<PropertyParty> PropertyParties { get; set; }

    /// <summary>
    /// Gets or sets rental charge policy entities.
    /// </summary>
    public virtual DbSet<RentalChargePolicy> RentalChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets unit package entities.
    /// </summary>
    public virtual DbSet<UnitPackage> UnitPackages { get; set; }

    /// <summary>
    /// Gets or sets unit package item entities.
    /// </summary>
    public virtual DbSet<UnitPackageItem> UnitPackageItems { get; set; }

    /// <summary>
    /// Configures Haven business table mappings.
    /// </summary>
    /// <param name="modelBuilder">The EF model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HavenDbContext).Assembly);

        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// Allows partial configuration extension.
    /// </summary>
    /// <param name="modelBuilder">The EF model builder.</param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
