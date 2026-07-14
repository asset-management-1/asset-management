namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures utility evidence document metadata.
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    /// <summary>
    /// Applies document metadata columns and public-id index.
    /// </summary>
    /// <param name="entity">The document entity builder.</param>
    public void Configure(EntityTypeBuilder<Document> entity)
    {
        entity.ToTable("Documents", "core");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.OriginalFileName).HasMaxLength(255);
        entity.Property(x => x.ContentType).HasMaxLength(150);
        entity.Property(x => x.FileExtension).HasMaxLength(20);
        entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.FileUrl).HasMaxLength(2000);
        entity.Property(x => x.Checksum).HasMaxLength(255);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.HasIndex(x => x.PublicId).IsUnique();
    }
}
