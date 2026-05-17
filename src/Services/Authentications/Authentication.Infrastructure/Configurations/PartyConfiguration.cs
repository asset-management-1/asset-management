namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the core.Parties table mapping used by user party contexts.
/// </summary>
public class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    /// <summary>
    /// Configures party indexes, profile columns, and master-data relationships.
    /// </summary>
    /// <param name="entity">The party entity type builder.</param>
    public void Configure(EntityTypeBuilder<Party> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Parties__3214EC07C86AC5D6");

        entity.ToTable("Parties", "core");

        entity.HasIndex(e => e.DisplayName, "IX_Core_Parties_DisplayName").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.PrimaryEmail, "IX_Core_Parties_PrimaryEmail").HasFilter(PRIMARY_EMAIL_FILTER);

        entity.HasIndex(e => e.PrimaryPhone, "IX_Core_Parties_PrimaryPhone").HasFilter(PRIMARY_PHONE_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_Core_Parties_PublicId").IsUnique();

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.DisplayName)
              .IsRequired()
              .HasMaxLength(255);

        entity.Property(e => e.PrimaryEmail).HasMaxLength(255);

        entity.Property(e => e.PrimaryPhone).HasMaxLength(50);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.HasOne(d => d.PartyType).WithMany(p => p.PartyPartyTypes)
              .HasForeignKey(d => d.PartyTypeId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_Parties_PartyTypeId");

        entity.HasOne(d => d.Status).WithMany(p => p.PartyStatuses)
              .HasForeignKey(d => d.StatusId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_Parties_StatusId");
    }
}
