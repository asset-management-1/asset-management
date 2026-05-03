namespace Authentication.Infrastructure.Configurations;

public class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Parties__3214EC07C86AC5D6");

        entity.ToTable("Parties", "core");

        entity.HasIndex(e => e.DisplayName, "IX_Core_Parties_DisplayName").HasFilter("([IsDeleted]=(0))");

        entity.HasIndex(e => e.PrimaryEmail, "IX_Core_Parties_PrimaryEmail").HasFilter("([PrimaryEmail] IS NOT NULL AND [IsDeleted]=(0))");

        entity.HasIndex(e => e.PrimaryPhone, "IX_Core_Parties_PrimaryPhone").HasFilter("([PrimaryPhone] IS NOT NULL AND [IsDeleted]=(0))");

        entity.HasIndex(e => e.PublicId, "UQ_Core_Parties_PublicId").IsUnique();

        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.DisplayName)
              .IsRequired()
              .HasMaxLength(255);
        entity.Property(e => e.LegalName).HasMaxLength(255);
        entity.Property(e => e.PrimaryEmail).HasMaxLength(255);
        entity.Property(e => e.PrimaryPhone).HasMaxLength(50);
        entity.Property(e => e.PublicId).HasDefaultValueSql("(newsequentialid())");

        entity.HasOne(d => d.PartyType).WithMany(p => p.PartyPartyTypes)
              .HasForeignKey(d => d.PartyTypeId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_Parties_PartyTypeId");

        entity.HasOne(d => d.Status).WithMany(p => p.PartyStatuses)
              .HasForeignKey(d => d.StatusId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Core_Parties_StatusId");
    }
}
