namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the core.PartyVehicles table mapping.
/// </summary>
public class PartyVehicleConfiguration : IEntityTypeConfiguration<PartyVehicle>
{
    /// <summary>
    /// Applies the EF Core table, property, relationship, and index mapping.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<PartyVehicle> entity)
    {
        entity.ToTable("PartyVehicles", "core");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.VehicleName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.LicensePlate).HasMaxLength(50);
        entity.Property(x => x.RegistrationFrontImageUrl).HasMaxLength(2000);
        entity.Property(x => x.RegistrationSideImageUrl).HasMaxLength(2000);
        entity.Property(x => x.VehicleFrontImageUrl).HasMaxLength(2000);
        entity.Property(x => x.VehicleSideImageUrl).HasMaxLength(2000);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => x.PartyId).HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => x.UnitId).HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => x.VehicleTypeId).HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.UnitId, x.PartyId }).HasFilter("\"IsDeleted\" = FALSE");
        entity.HasIndex(x => new { x.UnitId, x.VehicleTypeId }).HasFilter("\"IsDeleted\" = FALSE");
    }
}
