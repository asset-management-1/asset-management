namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the core.PartyIdentifiers table mapping.
/// </summary>
public class PartyIdentifierConfiguration : IEntityTypeConfiguration<PartyIdentifier>
{
    /// <summary>
    /// Configures party-identifier columns, indexes, and relationships.
    /// </summary>
    /// <param name="entity">The party-identifier entity type builder.</param>
    public void Configure(EntityTypeBuilder<PartyIdentifier> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__PartyIdentifiers__3214EC07145EBCB7");

        entity.ToTable("PartyIdentifiers", "core");

        entity.HasIndex(e => new { e.PartyId, e.IdentifierTypeId }, "IX_Core_PartyIdentifiers_PartyId")
            .HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_Core_PartyIdentifiers_PublicId").IsUnique();

        entity.HasIndex(e => e.StatusId, "IX_Core_PartyIdentifiers_StatusId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => new { e.IdentifierTypeId, e.IdentifierValue }, "UQ_Core_PartyIdentifiers_Type_Value")
            .IsUnique();

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.DateOfBirthOnDocument).HasColumnType(DATE_COLUMN_TYPE);

        entity.Property(e => e.ExpiredDate).HasColumnType(DATE_COLUMN_TYPE);

        entity.Property(e => e.FullNameOnDocument).HasMaxLength(255);

        entity.Property(e => e.IdentifierValue)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(e => e.IssuedBy).HasMaxLength(150);

        entity.Property(e => e.IssuedDate).HasColumnType(DATE_COLUMN_TYPE);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.Property(e => e.RegisteredAddress).HasMaxLength(500);

        entity.HasOne(d => d.Party).WithMany()
              .HasForeignKey(d => d.PartyId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_PartyIdentifiers_PartyId");

        entity.HasOne(d => d.IdentifierType).WithMany()
              .HasForeignKey(d => d.IdentifierTypeId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_PartyIdentifiers_IdentifierTypeId");

        entity.HasOne(d => d.GenderOnDocument).WithMany()
              .HasForeignKey(d => d.GenderOnDocumentId)
              .HasConstraintName("FK_Core_PartyIdentifiers_GenderOnDocumentId");

        entity.HasOne(d => d.Status).WithMany()
              .HasForeignKey(d => d.StatusId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_PartyIdentifiers_StatusId");
    }
}
