namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.ExternalLogins table mapping.
/// </summary>
public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    /// <summary>
    /// Configures external-login indexes, columns, and user relationship mapping.
    /// </summary>
    /// <param name="entity">The external-login entity type builder.</param>
    public void Configure(EntityTypeBuilder<ExternalLogin> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__External__3214EC07FBAEA69E");

        entity.ToTable("ExternalLogins", "identity");

        entity.HasIndex(e => e.UserId, "IX_Identity_ExternalLogins_UserId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => new { e.LoginProvider, e.ProviderKey }, "UQ_Identity_ExternalLogins_Provider_Key").IsUnique();

        entity.HasIndex(e => e.PublicId, "UQ_Identity_ExternalLogins_PublicId").IsUnique();

        entity.HasIndex(e => new { e.UserId, e.LoginProvider }, "UQ_Identity_ExternalLogins_User_Provider").IsUnique();

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.LoginProvider)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.ProviderKey)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.HasOne(d => d.User).WithMany(p => p.ExternalLogins)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_ExternalLogins_UserId");
    }
}
