namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.RefreshTokens table mapping.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// Configures refresh-token hashes, rotation metadata, and user relationship mapping.
    /// </summary>
    /// <param name="entity">The refresh-token entity type builder.</param>
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07750712E4");

        entity.ToTable("RefreshTokens", "identity");

        entity.HasIndex(e => e.PublicId, "UQ_Identity_RefreshTokens_PublicId").IsUnique();

        entity.HasIndex(e => new { e.UserId, e.DeviceId }, "UQ_Identity_RefreshTokens_User_Device")
            .IsUnique()
            .HasFilter(REFRESH_TOKEN_DEVICE_FILTER);

        entity.HasIndex(e => new { e.UserId, e.SessionPublicId }, "IX_Identity_RefreshTokens_User_SessionPublicId")
            .HasFilter(REFRESH_TOKEN_SESSION_PUBLIC_ID_FILTER);

        entity.HasIndex(e => new { e.UserId, e.CurrentPartyId }, "IX_Identity_RefreshTokens_User_CurrentPartyId")
            .HasFilter(NOT_DELETED_FILTER);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.CurrentPartyId).IsRequired();

        entity.Property(e => e.DeviceId).HasMaxLength(ClientDeviceMetadataLimits.DEVICE_ID_MAX_LENGTH);

        entity.Property(e => e.DeviceName).HasMaxLength(ClientDeviceMetadataLimits.DEVICE_NAME_MAX_LENGTH);

        entity.Property(e => e.DeviceType).HasMaxLength(ClientDeviceMetadataLimits.DEVICE_TYPE_MAX_LENGTH);

        entity.Property(e => e.IpAddress).HasMaxLength(ClientDeviceMetadataLimits.IP_ADDRESS_MAX_LENGTH);

        entity.Property(e => e.JwtId).HasMaxLength(150);

        entity.Property(e => e.PreviousTokenHash).HasMaxLength(500);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(500);

        entity.Property(e => e.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(e => e.UserAgent).HasMaxLength(ClientDeviceMetadataLimits.USER_AGENT_MAX_LENGTH);

        entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Identity_RefreshTokens_UserId");

        entity.HasOne(d => d.CurrentUserParty).WithMany()
            .HasForeignKey(d => new { d.UserId, d.CurrentPartyId })
            .HasPrincipalKey(p => new { p.UserId, p.PartyId })
            .HasConstraintName("FK_Identity_RefreshTokens_CurrentUserParty");
    }
}
