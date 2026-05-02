namespace Authentication.Infrastructure.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC07753F5F77");

        entity.ToTable("UserRoles", "identity");

        entity.HasIndex(e => e.RoleId, "IX_Identity_UserRoles_RoleId").HasFilter("([IsDeleted]=(0))");

        entity.HasIndex(e => e.UserId, "IX_Identity_UserRoles_UserId").HasFilter("([IsDeleted]=(0))");

        entity.HasIndex(e => e.PublicId, "UQ_Identity_UserRoles_PublicId").IsUnique();

        entity.HasIndex(e => new { e.UserId, e.RoleId }, "UX_Identity_UserRoles_User_Role")
            .IsUnique()
            .HasFilter("([IsDeleted]=(0))");

        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.PublicId).HasDefaultValueSql("(newsequentialid())");

        entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_UserRoles_RoleId");

        entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_UserRoles_UserId");
    }
}
