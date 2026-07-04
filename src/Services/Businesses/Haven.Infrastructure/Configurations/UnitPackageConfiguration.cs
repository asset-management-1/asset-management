namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the asset.UnitPackages table mapping.
/// </summary>
public class UnitPackageConfiguration : IEntityTypeConfiguration<UnitPackage>
{
    /// <summary>
    /// Applies unit-package mapping to the EF model.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UnitPackage> entity)
    {
        entity.ToTable("UnitPackages", "asset");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.PackageCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.PackageName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.Property(x => x.PriceAdjustment).HasPrecision(18, 2).HasDefaultValue(0);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasOne(x => x.Unit)
            .WithMany(x => x.UnitPackages)
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.UnitId, x.PackageCode }).IsUnique();
        entity.HasIndex(x => x.UnitId).HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.UnitId, x.PackageTypeId }).HasFilter("\"IsDeleted\" = FALSE");
    }
}
