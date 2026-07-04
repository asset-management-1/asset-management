namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the asset.PropertyParties table mapping.
/// </summary>
public class PropertyPartyConfiguration : IEntityTypeConfiguration<PropertyParty>
{
    /// <summary>
    /// Applies property-party mapping to the EF model.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<PropertyParty> entity)
    {
        entity.ToTable("PropertyParties", "asset");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.Note).HasMaxLength(1000);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasOne(x => x.Property)
            .WithMany(x => x.PropertyParties)
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.PropertyId, x.PartyId, x.RelationshipTypeId, x.StartDate })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.PropertyId, x.RelationshipTypeId })
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.PartyId, x.RelationshipTypeId })
            .HasFilter("\"IsDeleted\" = FALSE");
    }
}
