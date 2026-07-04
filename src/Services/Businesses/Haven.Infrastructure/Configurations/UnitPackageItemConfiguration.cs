namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the asset.UnitPackageItems table mapping.
/// </summary>
public class UnitPackageItemConfiguration : IEntityTypeConfiguration<UnitPackageItem>
{
    /// <summary>
    /// Applies unit-package-item mapping to the EF model.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UnitPackageItem> entity)
    {
        entity.ToTable("UnitPackageItems", "asset");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.ItemName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.DisplayOrder).HasDefaultValue(0);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasOne(x => x.UnitPackage)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.UnitPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.UnitPackageId, x.DisplayOrder }).HasFilter("\"IsDeleted\" = FALSE");
    }
}
