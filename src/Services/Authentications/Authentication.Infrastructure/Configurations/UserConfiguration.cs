namespace Authentication.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07726AA6D7");

        entity.ToTable("Users", "identity");

        entity.HasIndex(e => e.LockoutEndAt, "IX_Identity_Users_Lockout").HasFilter("([LockoutEndAt] IS NOT NULL AND [IsDeleted]=(0))");

        entity.HasIndex(e => new { e.Email, e.StatusId }, "IX_Identity_Users_Login").HasFilter("([IsDeleted]=(0))");

        entity.HasIndex(e => e.PartyId, "IX_Identity_Users_PartyId").HasFilter("([IsDeleted]=(0))");

        entity.HasIndex(e => e.PublicId, "UQ_Identity_Users_PublicId").IsUnique();

        entity.HasIndex(e => e.Email, "UX_Identity_Users_Email")
            .IsUnique()
            .HasFilter("([Email] IS NOT NULL AND [IsDeleted]=(0))");

        entity.HasIndex(e => e.PhoneNumber, "UX_Identity_Users_Phone")
            .IsUnique()
            .HasFilter("([PhoneNumber] IS NOT NULL AND [IsDeleted]=(0))");

        entity.HasIndex(e => e.UserName, "UX_Identity_Users_UserName")
            .IsUnique()
            .HasFilter("([UserName] IS NOT NULL AND [IsDeleted]=(0))");

        entity.Property(e => e.AvatarUrl).HasMaxLength(1000);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Email).HasMaxLength(255);
        entity.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(255);
        entity.Property(e => e.LockoutEnabled).HasDefaultValue(true);
        entity.Property(e => e.PasswordHash).HasMaxLength(500);
        entity.Property(e => e.PhoneNumber).HasMaxLength(50);
        entity.Property(e => e.PublicId).HasDefaultValueSql("(newsequentialid())");
        entity.Property(e => e.UserName)
            .IsRequired()
            .HasMaxLength(255);

        entity.HasOne(d => d.Status).WithMany(p => p.Users)
            .HasForeignKey(d => d.StatusId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_Users_StatusId");
    }
}
