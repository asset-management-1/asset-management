namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

public sealed class CoreRepositoryDbContext : DbContext
{
    public CoreRepositoryDbContext(DbContextOptions<CoreRepositoryDbContext> options) : base(options)
    {
    }

    public DbSet<CoreRepositorySampleEntity> Samples => Set<CoreRepositorySampleEntity>();

    public DbSet<CoreAuditableSampleEntity> AuditableSamples => Set<CoreAuditableSampleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CoreRepositorySampleEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<CoreAuditableSampleEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.Name).HasMaxLength(120);
        });
    }
}
