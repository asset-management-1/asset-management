namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.RolePermissions table mapping.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    /// <summary>
    /// Configures role-permission uniqueness and relationship mapping.
    /// </summary>
    /// <param name="entity">The role-permission entity type builder.</param>
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__RolePerm__3214EC07C0BD1AF2");

        entity.ToTable("RolePermissions", "identity");

        entity.HasIndex(e => e.PermissionId, "IX_Identity_RolePermissions_PermissionId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.RoleId, "IX_Identity_RolePermissions_RoleId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_Identity_RolePermissions_PublicId").IsUnique();

        entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "UX_Identity_RolePermissions_Role_Permission")
            .IsUnique()
            .HasFilter(NOT_DELETED_FILTER);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
            .HasForeignKey(d => d.PermissionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_RolePermissions_PermissionId");

        entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_RolePermissions_RoleId");
    }
}
