namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.Permissions table mapping.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    /// <summary>
    /// Configures permission indexes and stable code/name columns.
    /// </summary>
    /// <param name="entity">The permission entity type builder.</param>
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC07D13F6F95");

        entity.ToTable("Permissions", "identity");

        entity.HasIndex(e => new { e.Module, e.StatusId }, "IX_Identity_Permissions_Module").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.Code, "UQ_Identity_Permissions_Code").IsUnique();

        entity.HasIndex(e => e.PublicId, "UQ_Identity_Permissions_PublicId").IsUnique();

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.Description).HasMaxLength(500);

        entity.Property(e => e.Module).HasMaxLength(100);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.HasOne(d => d.Status).WithMany(p => p.Permissions)
            .HasForeignKey(d => d.StatusId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_Permissions_StatusId");
    }
}
