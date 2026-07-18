namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the leasing.Occupancies table mapping.
/// </summary>
public class OccupancyConfiguration : IEntityTypeConfiguration<Occupancy>
{
    /// <summary>
    /// Applies the EF Core table, property, relationship, and index mapping.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Occupancy> entity)
    {
        entity.ToTable("Occupancies", "leasing");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.IsPrimaryTenant).HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasOne(x => x.Contract)
            .WithMany(x => x.Occupancies)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Party)
            .WithMany(x => x.Occupancies)
            .HasForeignKey(x => x.PartyId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Unit)
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => new { x.UnitId, x.StatusId, x.StartDate, x.EndDate })
            .HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.UnitId, x.IsPrimaryTenant, x.StatusId, x.StartDate, x.EndDate })
            .HasFilter("\"IsDeleted\" = FALSE");
    }
}
