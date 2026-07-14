namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.UserParties table mapping.
/// </summary>
public class UserPartyConfiguration : IEntityTypeConfiguration<UserParty>
{
    /// <summary>
    /// Applies the EF Core table, property, relationship, and index mapping.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<UserParty> entity)
    {
        entity.ToTable("UserParties", "identity");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasOne(x => x.User)
            .WithMany(x => x.UserParties)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Party)
            .WithMany(x => x.UserParties)
            .HasForeignKey(x => x.PartyId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.UserId, x.PartyId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");
    }
}
