namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Configures the identity.Users table mapping.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures user profile columns, indexes, and master-data relationships.
    /// </summary>
    /// <param name="entity">The user entity type builder.</param>
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07726AA6D7");

        entity.ToTable("Users", "identity");

        entity.HasIndex(e => new { e.UserName, e.StatusId }, "IX_Identity_Users_Login").HasFilter(NOT_DELETED_FILTER);

        entity.HasIndex(e => e.PublicId, "UQ_Identity_Users_PublicId").IsUnique();

        entity.HasIndex(e => e.Email, "UX_Identity_Users_Email")
            .IsUnique()
            .HasFilter(EMAIL_FILTER);

        entity.HasIndex(e => e.GenderId, "IX_Identity_Users_GenderId").HasFilter(GENDER_ID_FILTER);

        entity.HasIndex(e => e.PhoneNumber, "UX_Identity_Users_Phone")
            .IsUnique()
            .HasFilter(PHONE_NUMBER_FILTER);

        entity.HasIndex(e => e.UserName, "UX_Identity_Users_UserName")
            .IsUnique()
            .HasFilter(USER_NAME_FILTER);

        entity.Property(e => e.AvatarUrl).HasMaxLength(1000);

        entity.Property(e => e.AuthResetAt);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql(CURRENT_TIMESTAMP_SQL);

        entity.Property(e => e.DateOfBirth).HasColumnType(DATE_COLUMN_TYPE);

        entity.Property(e => e.Email).HasMaxLength(255);

        entity.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.PasswordHash).HasMaxLength(500);

        entity.Property(e => e.PhoneNumber).HasMaxLength(50);

        entity.Property(e => e.PublicId).HasDefaultValueSql(GENERATED_UUID_SQL);

        entity.Property(e => e.UserName)
            .IsRequired()
            .HasMaxLength(255);

        entity.HasOne(d => d.Gender).WithMany()
              .HasForeignKey(d => d.GenderId)
              .HasConstraintName("FK_Identity_Users_GenderId");

        entity.HasOne(d => d.Status).WithMany(p => p.Users)
              .HasForeignKey(d => d.StatusId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Identity_Users_StatusId");
    }
}
