namespace Authentication.Infrastructure.Configurations;

public class MasterDataValueConfiguration : IEntityTypeConfiguration<MasterDataValue>
{
    public void Configure(EntityTypeBuilder<MasterDataValue> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__MasterDa__3214EC0719E62DB0");

        entity.ToTable("MasterDataValues", "masterdata");

        entity.HasIndex(e => new { e.MasterDataTypeId, e.IsActive }, "IX_MasterData_MasterDataValues_MasterDataTypeId").HasFilter("([IsDeleted]=(0))");

        entity.HasIndex(e => e.PublicId, "UQ_MasterData_MasterDataValues_PublicId").IsUnique();

        entity.HasIndex(e => new { e.MasterDataTypeId, e.Code }, "UQ_MasterData_MasterDataValues_Type_Code").IsUnique();

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        entity.Property(e => e.PublicId).HasDefaultValueSql("(newsequentialid())");

        entity.HasOne(d => d.MasterDataType).WithMany(p => p.MasterDataValues)
            .HasForeignKey(d => d.MasterDataTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_MasterData_MasterDataValues_MasterDataTypeId");
    }
}
