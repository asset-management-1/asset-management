namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.UserParties mapping table for user party contexts.
/// </summary>
public class UserPartyConfiguration : IEntityTypeConfiguration<UserParty>
{
    /// <summary>
    /// Configures user-party uniqueness, audit columns, and party/user relationships.
    /// </summary>
    /// <param name="entity">The user-party entity type builder.</param>
    public void Configure(EntityTypeBuilder<UserParty> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__UserPart__3214EC07E0B2C8A1");

        entity.ToTable("UserParties", "identity");

        entity.HasIndex(e => e.PartyId, "IX_Identity_UserParties_PartyId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.UserId, "IX_Identity_UserParties_UserId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_Identity_UserParties_PublicId").IsUnique();

        // The permanent pair key lets a session reference only a Party linked to the same User.
        entity.HasAlternateKey(e => new { e.UserId, e.PartyId })
            .HasName("AK_Identity_UserParties_User_Party");

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.HasOne(d => d.Party).WithMany(p => p.UserParties)
            .HasForeignKey(d => d.PartyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_UserParties_PartyId");

        entity.HasOne(d => d.User).WithMany(p => p.UserParties)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_UserParties_UserId");
    }
}
