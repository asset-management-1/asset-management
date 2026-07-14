namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures invoice line persistence.
/// </summary>
public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    /// <summary>
    /// Applies invoice-line columns and parent relationship.
    /// </summary>
    /// <param name="entity">The invoice-line entity builder.</param>
    public void Configure(EntityTypeBuilder<InvoiceLine> entity)
    {
        entity.ToTable("InvoiceLines", "billing");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Note).HasMaxLength(1000);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasOne(x => x.Invoice).WithMany(x => x.InvoiceLines)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
