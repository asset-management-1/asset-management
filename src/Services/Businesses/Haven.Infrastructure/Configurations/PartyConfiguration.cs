namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the core.Parties table mapping.
/// </summary>
public class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    /// <summary>
    /// Applies the EF Core table, property, relationship, and index mapping.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Party> entity)
    {
        entity.ToTable("Parties", "core");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.DisplayName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.PrimaryPhone).HasMaxLength(50);
        entity.Property(x => x.PrimaryEmail).HasMaxLength(255);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => x.DisplayName).HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => x.PrimaryPhone).HasFilter("\"PrimaryPhone\" IS NOT NULL AND \"IsDeleted\" = FALSE");
        entity.HasIndex(x => x.PrimaryEmail).HasFilter("\"PrimaryEmail\" IS NOT NULL AND \"IsDeleted\" = FALSE");
    }
}
