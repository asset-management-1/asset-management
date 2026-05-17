namespace Authentication.Infrastructure.Context;

/// <summary>
/// Represents the Entity Framework database context for the authentication module.
/// </summary>
public partial class AuthenticationDbContext : DbContext
{
    /// <summary>
    /// Creates the authentication EF Core context with the configured database options.
    /// </summary>
    /// <param name="options">The database context options configured by dependency injection.</param>
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ExternalLogin> ExternalLogins { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentLink> DocumentLinks { get; set; }

    public virtual DbSet<MasterDataType> MasterDataTypes { get; set; }

    public virtual DbSet<MasterDataValue> MasterDataValues { get; set; }

    public virtual DbSet<Party> Parties { get; set; }

    public virtual DbSet<PartyVehicle> PartyVehicles { get; set; }

    public virtual DbSet<PartyIdentifier> PartyIdentifiers { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserParty> UserParties { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    /// <summary>
    /// Configures the authentication model mappings.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthenticationDbContext).Assembly);

        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// Provides a partial extension point for additional model configuration.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
