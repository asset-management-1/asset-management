namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures invoice replacement lineage and contract-period indexes.
/// </summary>
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    /// <summary>
    /// Applies invoice columns, indexes, and self-referencing replacement relationship.
    /// </summary>
    /// <param name="entity">The invoice entity builder.</param>
    public void Configure(EntityTypeBuilder<Invoice> entity)
    {
        entity.ToTable("Invoices", "billing");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.InvoiceCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
        entity.Property(x => x.BalanceAmount).HasPrecision(18, 2);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => x.InvoiceCode).IsUnique();
        entity.HasIndex(x => x.ReplacesInvoiceId).HasFilter("\"ReplacesInvoiceId\" IS NOT NULL");
        entity.HasIndex(x => new { x.ContractId, x.BillingPeriodFrom, x.BillingPeriodTo, x.StatusId })
            .HasFilter("\"ContractId\" IS NOT NULL AND \"IsDeleted\" = FALSE");

        entity.HasOne(x => x.Contract).WithMany(x => x.Invoices)
            .HasForeignKey(x => x.ContractId);
        entity.HasOne(x => x.ReplacesInvoice).WithMany(x => x.ReplacementInvoices)
            .HasForeignKey(x => x.ReplacesInvoiceId);
    }
}
