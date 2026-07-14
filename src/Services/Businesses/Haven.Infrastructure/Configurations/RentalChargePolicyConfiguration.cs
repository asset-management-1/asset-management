namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the asset.RentalChargePolicies table mapping.
/// </summary>
public class RentalChargePolicyConfiguration : IEntityTypeConfiguration<RentalChargePolicy>
{
    /// <summary>
    /// Applies rental charge policy mapping to the EF model.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<RentalChargePolicy> entity)
    {
        entity.ToTable("RentalChargePolicies", "asset");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.ChargeName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 4).HasDefaultValue(0);
        entity.Property(x => x.IsUsageBased).HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasOne(x => x.Property)
            .WithMany(x => x.RentalChargePolicies)
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Unit)
            .WithMany(x => x.RentalChargePolicies)
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.PropertyId, x.StatusId })
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.PropertyId, x.LineTypeId, x.VehicleTypeId })
            .HasFilter("\"UnitId\" IS NULL AND \"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.UnitId, x.LineTypeId, x.VehicleTypeId })
            .HasFilter("\"UnitId\" IS NOT NULL AND \"IsDeleted\" = FALSE");
    }
}
