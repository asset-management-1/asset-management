namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the core.DocumentLinks table mapping.
/// </summary>
public class DocumentLinkConfiguration : IEntityTypeConfiguration<DocumentLink>
{
    /// <summary>
    /// Configures document-link columns, indexes, and relationships.
    /// </summary>
    /// <param name="entity">The document-link entity type builder.</param>
    public void Configure(EntityTypeBuilder<DocumentLink> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__DocumentLinks__3214EC0783E7E561");

        entity.ToTable("DocumentLinks", "core");

        entity.HasIndex(e => e.DocumentId, "IX_Core_DocumentLinks_DocumentId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => new { e.EntityTypeId, e.EntityId, e.IsPrimary, e.SortOrder }, "IX_Core_DocumentLinks_Entity")
            .HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_Core_DocumentLinks_PublicId").IsUnique();

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.IsPrimary).HasDefaultValue(false);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.Property(e => e.SortOrder).HasDefaultValue(0);

        entity.HasOne(d => d.Document).WithMany(p => p.DocumentLinks)
              .HasForeignKey(d => d.DocumentId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_DocumentLinks_DocumentId");

        entity.HasOne(d => d.EntityType).WithMany()
              .HasForeignKey(d => d.EntityTypeId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_DocumentLinks_EntityTypeId");

        entity.HasOne(d => d.LinkType).WithMany()
              .HasForeignKey(d => d.LinkTypeId)
              .HasConstraintName("FK_Core_DocumentLinks_LinkTypeId");

        entity.HasOne(d => d.Status).WithMany()
              .HasForeignKey(d => d.StatusId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_DocumentLinks_StatusId");
    }
}
