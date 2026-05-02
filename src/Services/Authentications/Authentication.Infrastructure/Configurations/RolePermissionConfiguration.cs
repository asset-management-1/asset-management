namespace Authentication.Infrastructure.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__RolePerm__3214EC07C0BD1AF2");

        entity.ToTable("RolePermissions", "identity");

        entity.HasIndex(e => e.PermissionId, "IX_Identity_RolePermissions_PermissionId").HasFilter("([IsDeleted]=(0))");

        entity.HasIndex(e => e.RoleId, "IX_Identity_RolePermissions_RoleId").HasFilter("([IsDeleted]=(0))");

        entity.HasIndex(e => e.PublicId, "UQ_Identity_RolePermissions_PublicId").IsUnique();

        entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "UX_Identity_RolePermissions_Role_Permission")
            .IsUnique()
            .HasFilter("([IsDeleted]=(0))");

        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.PublicId).HasDefaultValueSql("(newsequentialid())");

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
