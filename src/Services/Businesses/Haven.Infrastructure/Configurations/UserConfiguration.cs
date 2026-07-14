namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.Users table mapping.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Applies the EF Core table, property, relationship, and index mapping.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("Users", "identity");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.UserName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(255);
        entity.Property(x => x.PhoneNumber).HasMaxLength(50);
        entity.Property(x => x.FullName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.AvatarUrl).HasMaxLength(1000);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => x.CurrentPartyId).HasFilter("\"CurrentPartyId\" IS NOT NULL AND \"IsDeleted\" = FALSE");
    }
}
