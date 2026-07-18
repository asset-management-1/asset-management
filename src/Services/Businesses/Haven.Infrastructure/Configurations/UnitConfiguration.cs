namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the asset.Units table mapping.
/// </summary>
public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    /// <summary>
    /// Applies unit mapping to the EF model.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Unit> entity)
    {
        entity.ToTable("Units", "asset");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.UnitCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.UnitName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.AreaSqm).HasPrecision(18, 2);
        entity.Property(x => x.BaseRentAmount).HasPrecision(18, 2).HasDefaultValue(0);
        entity.Property(x => x.DefaultDepositAmount).HasPrecision(18, 2).HasDefaultValue(0);
        entity.Property(x => x.IsPetAllowed).HasDefaultValue(false);
        entity.Property(x => x.IsPublished).HasDefaultValue(false);
        entity.Property(x => x.Note).HasMaxLength(1000);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasOne(x => x.Property)
            .WithMany(x => x.Units)
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.PropertyId, x.UnitCode }).IsUnique();
        entity.HasIndex(x => new { x.PropertyId, x.StatusId, x.IsPublished })
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.PropertyId, x.FloorNumber, x.StatusId })
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => x.UnitName, "IX_Asset_Units_UnitName_Trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasFilter("\"IsDeleted\" = FALSE");
    }
}
