namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the masterdata.MasterDataTypes table mapping.
/// </summary>
public class MasterDataTypeConfiguration : IEntityTypeConfiguration<MasterDataType>
{
    /// <summary>
    /// Configures master-data type indexes and column mappings.
    /// </summary>
    /// <param name="entity">The master-data type entity builder.</param>
    public void Configure(EntityTypeBuilder<MasterDataType> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__MasterDa__3214EC079C9A46A1");

        entity.ToTable("MasterDataTypes", "masterdata");

        entity.HasIndex(e => e.Code, "UQ_MasterData_MasterDataTypes_Code").IsUnique();

        entity.HasIndex(e => e.PublicId, "UQ_MasterData_MasterDataTypes_PublicId").IsUnique();

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.Description).HasMaxLength(500);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);
    }
}
