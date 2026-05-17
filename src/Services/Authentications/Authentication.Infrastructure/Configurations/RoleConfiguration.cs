namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.Roles table mapping.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    /// <summary>
    /// Configures role indexes, display columns, and party-type relationship mapping.
    /// </summary>
    /// <param name="entity">The role entity type builder.</param>
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC073C9548FB");

        entity.ToTable("Roles", "identity");

        entity.HasIndex(e => e.StatusId, "IX_Identity_Roles_StatusId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.Code, "UQ_Identity_Roles_Code").IsUnique();

        entity.HasIndex(e => e.PublicId, "UQ_Identity_Roles_PublicId").IsUnique();

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.Description).HasMaxLength(500);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.HasOne(d => d.Status).WithMany(p => p.Roles)
            .HasForeignKey(d => d.StatusId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_Roles_StatusId");
    }
}
