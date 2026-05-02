namespace Authentication.Infrastructure.Configurations;

public class MasterDataTypeConfiguration : IEntityTypeConfiguration<MasterDataType>
{
    public void Configure(EntityTypeBuilder<MasterDataType> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__MasterDa__3214EC079C9A46A1");

        entity.ToTable("MasterDataTypes", "masterdata");

        entity.HasIndex(e => e.Code, "UQ_MasterData_MasterDataTypes_Code").IsUnique();

        entity.HasIndex(e => e.PublicId, "UQ_MasterData_MasterDataTypes_PublicId").IsUnique();

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        entity.Property(e => e.PublicId).HasDefaultValueSql("(newsequentialid())");
    }
}
