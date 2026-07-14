namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures utility snapshots attached to invoice lines.
/// </summary>
public class InvoiceLineUtilityConfiguration : IEntityTypeConfiguration<InvoiceLineUtility>
{
    /// <summary>
    /// Applies snapshot precision and one-to-one invoice-line mapping.
    /// </summary>
    /// <param name="entity">The utility snapshot entity builder.</param>
    public void Configure(EntityTypeBuilder<InvoiceLineUtility> entity)
    {
        entity.ToTable("InvoiceLineUtilities", "billing");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.PreviousReading).HasPrecision(18, 3);
        entity.Property(x => x.CurrentReading).HasPrecision(18, 3);
        entity.Property(x => x.UsageQuantity).HasPrecision(18, 3);
        entity.Property(x => x.UnitPrice).HasPrecision(18, 4);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => x.InvoiceLineId).IsUnique();
        entity.HasOne(x => x.InvoiceLine).WithOne(x => x.Utility)
            .HasForeignKey<InvoiceLineUtility>(x => x.InvoiceLineId)
            .OnDelete(DeleteBehavior.ClientSetNull);
        entity.HasOne(x => x.Meter).WithMany()
            .HasForeignKey(x => x.MeterId);
    }
}
