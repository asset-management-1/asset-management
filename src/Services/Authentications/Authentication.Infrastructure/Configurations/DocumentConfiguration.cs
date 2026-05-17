namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the core.Documents table mapping.
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    /// <summary>
    /// Configures document metadata columns, indexes, and master-data relationships.
    /// </summary>
    /// <param name="entity">The document entity type builder.</param>
    public void Configure(EntityTypeBuilder<Document> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Documents__3214EC07F7A8E6A4");

        entity.ToTable("Documents", "core");

        entity.HasIndex(e => new { e.DocumentTypeId, e.StatusId }, "IX_Core_Documents_DocumentTypeId")
            .HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_Core_Documents_PublicId").IsUnique();

        entity.Property(e => e.Checksum).HasMaxLength(255);

        entity.Property(e => e.ContentType).HasMaxLength(150);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.FileExtension).HasMaxLength(20);

        entity.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.FileUrl).HasMaxLength(2000);

        entity.Property(e => e.OriginalFileName).HasMaxLength(255);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.Property(e => e.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        entity.HasOne(d => d.DocumentType).WithMany()
              .HasForeignKey(d => d.DocumentTypeId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_Documents_DocumentTypeId");

        entity.HasOne(d => d.StorageProvider).WithMany()
              .HasForeignKey(d => d.StorageProviderId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_Documents_StorageProviderId");

        entity.HasOne(d => d.Status).WithMany()
              .HasForeignKey(d => d.StatusId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_Documents_StatusId");
    }
}
