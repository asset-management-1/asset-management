namespace Authentication.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07750712E4");

        entity.ToTable("RefreshTokens", "identity");

        entity.HasIndex(e => e.PublicId, "UQ_Identity_RefreshTokens_PublicId").IsUnique();

        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.JwtId).HasMaxLength(150);
        entity.Property(e => e.PublicId).HasDefaultValueSql("(newsequentialid())");
        entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(500);
        entity.Property(e => e.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_RefreshTokens_UserId");
    }
}
