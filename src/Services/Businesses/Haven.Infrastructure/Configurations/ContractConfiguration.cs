namespace Haven.Infrastructure.Configurations;

/// <summary>
/// Configures the leasing.Contracts table mapping.
/// </summary>
public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    /// <summary>
    /// Applies the EF Core table, property, relationship, and index mapping.
    /// </summary>
    /// <param name="entity">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Contract> entity)
    {
        entity.ToTable("Contracts", "leasing");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityByDefaultColumn();
        entity.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        entity.Property(x => x.ContractCode).HasMaxLength(100).IsRequired();
        entity.Property(x => x.RentAmount).HasPrecision(18, 2);
        entity.Property(x => x.DepositAmount).HasPrecision(18, 2);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);

        entity.HasIndex(x => x.PublicId).IsUnique();
        entity.HasIndex(x => x.ContractCode).IsUnique();
        entity.HasIndex(x => x.UnitId).HasFilter("\"UnitId\" IS NOT NULL AND \"IsDeleted\" = FALSE");
        entity.HasIndex(x => x.SecondaryPartyId).HasFilter("\"SecondaryPartyId\" IS NOT NULL AND \"IsDeleted\" = FALSE");
    }
}
