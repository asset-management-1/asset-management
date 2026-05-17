namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the masterdata.MasterDataValues table mapping.
/// </summary>
public class MasterDataValueConfiguration : IEntityTypeConfiguration<MasterDataValue>
{
    /// <summary>
    /// Configures master-data value indexes, columns, and type relationship mapping.
    /// </summary>
    /// <param name="entity">The master-data value entity builder.</param>
    public void Configure(EntityTypeBuilder<MasterDataValue> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__MasterDa__3214EC0719E62DB0");

        entity.ToTable("MasterDataValues", "masterdata");

        entity.HasIndex(e => new { e.MasterDataTypeId, e.IsActive }, "IX_MasterData_MasterDataValues_MasterDataTypeId").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_MasterData_MasterDataValues_PublicId").IsUnique();

        entity.HasIndex(e => new { e.MasterDataTypeId, e.Code }, "UQ_MasterData_MasterDataValues_Type_Code").IsUnique();

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.Description).HasMaxLength(500);

        entity.Property(e => e.IsActive).HasDefaultValue(true);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.HasOne(d => d.MasterDataType).WithMany(p => p.MasterDataValues)
            .HasForeignKey(d => d.MasterDataTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_MasterData_MasterDataValues_MasterDataTypeId");
    }
}
