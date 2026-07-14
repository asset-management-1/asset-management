namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures evidence links from documents to meter records.
/// </summary>
public class DocumentLinkConfiguration : IEntityTypeConfiguration<DocumentLink>
{
    /// <summary>
    /// Applies document-link columns, ordering index, and document relationship.
    /// </summary>
    /// <param name="entity">The document-link entity builder.</param>
    public void Configure(EntityTypeBuilder<DocumentLink> entity)
    {
        entity.ToTable("DocumentLinks", "core");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.IsPrimary).HasDefaultValue(false);
        entity.Property(x => x.SortOrder).HasDefaultValue(0);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.EntityTypeId, x.EntityId, x.SortOrder })
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasOne(x => x.Document).WithMany(x => x.DocumentLinks)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
