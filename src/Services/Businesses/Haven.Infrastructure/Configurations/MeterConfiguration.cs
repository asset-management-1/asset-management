namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures monthly meter records and their property or room scope.
/// </summary>
public class MeterConfiguration : IEntityTypeConfiguration<Meter>
{
    /// <summary>
    /// Applies table, precision, relationship, and active-period uniqueness mappings.
    /// </summary>
    /// <param name="entity">The meter entity builder.</param>
    public void Configure(EntityTypeBuilder<Meter> entity)
    {
        entity.ToTable("Meters", "billing");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.PreviousReading).HasPrecision(18, 3);
        entity.Property(x => x.CurrentReading).HasPrecision(18, 3);
        entity.Property(x => x.UsageQuantity).HasPrecision(18, 3);
        entity.Property(x => x.UnitPriceSnapshot).HasPrecision(18, 4);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.PropertyId, x.LineTypeId, x.BillingPeriodFrom, x.BillingPeriodTo })
            .IsUnique()
            .HasFilter("\"UnitId\" IS NULL AND \"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.PropertyId, x.UnitId, x.LineTypeId, x.BillingPeriodFrom, x.BillingPeriodTo })
            .IsUnique()
            .HasFilter("\"UnitId\" IS NOT NULL AND \"IsDeleted\" = FALSE");

        entity.HasOne(x => x.Property).WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.ClientSetNull);
        entity.HasOne(x => x.Unit).WithMany()
            .HasForeignKey(x => x.UnitId);
        entity.HasOne(x => x.ChargePolicy).WithMany()
            .HasForeignKey(x => x.ChargePolicyId);
    }
}
