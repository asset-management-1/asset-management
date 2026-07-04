namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the asset.Properties table mapping.
/// </summary>
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    /// <summary>
    /// Applies property mapping to the EF model.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Property> entity)
    {
        entity.ToTable("Properties", "asset");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.PropertyCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(255).IsRequired();
        entity.Property(x => x.Description).HasColumnType("text");
        entity.Property(x => x.StreetAddress).HasMaxLength(1000);
        entity.Property(x => x.FormattedAddress).HasMaxLength(1000);
        entity.Property(x => x.Latitude).HasPrecision(18, 10);
        entity.Property(x => x.Longitude).HasPrecision(18, 10);
        entity.Property(x => x.IsPublished).HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasIndex(x => x.PropertyCode).IsUnique();
        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => x.Name)
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.ProvinceId, x.DistrictId, x.WardId })
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.StatusId, x.IsPublished })
            .HasFilter("\"IsDeleted\" = FALSE");
    }
}
