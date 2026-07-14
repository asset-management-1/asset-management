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
    /// Gets or sets party entities.
    /// </summary>
    public virtual DbSet<Party> Parties { get; set; }

    /// <summary>
    /// Gets or sets user account entities.
    /// </summary>
    public virtual DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets user-party link entities.
    /// </summary>
    public virtual DbSet<UserParty> UserParties { get; set; }

    /// <summary>
    /// Gets or sets rental contract entities.
    /// </summary>
    public virtual DbSet<Contract> Contracts { get; set; }

    /// <summary>
    /// Gets or sets occupancy entities.
    /// </summary>
    public virtual DbSet<Occupancy> Occupancies { get; set; }

    /// <summary>
    /// Gets or sets party vehicle entities.
    /// </summary>
    public virtual DbSet<PartyVehicle> PartyVehicles { get; set; }

    /// <summary>
    /// Gets or sets monthly meter records.
    /// </summary>
    public virtual DbSet<Meter> Meters { get; set; }

    /// <summary>
    /// Gets or sets invoices and replacement invoices.
    /// </summary>
    public virtual DbSet<Invoice> Invoices { get; set; }

    /// <summary>
    /// Gets or sets invoice lines.
    /// </summary>
    public virtual DbSet<InvoiceLine> InvoiceLines { get; set; }

    /// <summary>
    /// Gets or sets utility invoice snapshots.
    /// </summary>
    public virtual DbSet<InvoiceLineUtility> InvoiceLineUtilities { get; set; }

    /// <summary>
    /// Gets or sets evidence documents.
    /// </summary>
    public virtual DbSet<Document> Documents { get; set; }

    /// <summary>
    /// Gets or sets evidence document links.
    /// </summary>
    public virtual DbSet<DocumentLink> DocumentLinks { get; set; }

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
