namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the core.PartyVehicles table mapping used by tenant profile vehicles.
/// </summary>
public class PartyVehicleConfiguration : IEntityTypeConfiguration<PartyVehicle>
{
    /// <summary>
    /// Configures vehicle columns, tenant-scoped indexes, and master-data relationships.
    /// </summary>
    /// <param name="entity">The party-vehicle entity type builder.</param>
    public void Configure(EntityTypeBuilder<PartyVehicle> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK_Core_PartyVehicles");

        entity.ToTable("PartyVehicles", "core");

        entity.HasIndex(e => e.PartyId, "IX_Core_PartyVehicles_PartyId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.UnitId, "IX_Core_PartyVehicles_UnitId").HasFilter(UNIT_ID_FILTER);

        entity.HasIndex(e => e.VehicleTypeId, "IX_Core_PartyVehicles_VehicleTypeId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(
                e => new
                {
                    e.UnitId,
                    e.VehicleTypeId
                },
                "IX_Core_PartyVehicles_Unit_VehicleType_Status")
            .HasFilter(UNIT_ID_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_Core_PartyVehicles_PublicId").IsUnique();

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.FrontImageUrl).HasMaxLength(2000);

        entity.Property(e => e.LicensePlate).HasMaxLength(50);

        entity.Property(e => e.PlateImageUrl).HasMaxLength(2000);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.Property(e => e.SideImageUrl).HasMaxLength(2000);

        entity.Property(e => e.VehicleName)
            .IsRequired()
            .HasMaxLength(255);

        entity.HasOne(d => d.Party)
            .WithMany(p => p.PartyVehicles)
            .HasForeignKey(d => d.PartyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Core_PartyVehicles_PartyId");

        entity.HasOne(d => d.VehicleType)
            .WithMany()
            .HasForeignKey(d => d.VehicleTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Core_PartyVehicles_VehicleTypeId");
    }
}
