namespace Haven.Infrastructure.Repositories;

/// <summary>
/// Provides EF write operations for rental charge policies.
/// </summary>
public class RentalChargePolicyRepository : GenericRepository<RentalChargePolicy>, IRentalChargePolicyRepository
{
    /// <summary>
    /// Creates the rental charge policy repository.
    /// </summary>
    /// <param name="dbContext">The Haven business database context.</param>
    public RentalChargePolicyRepository(HavenDbContext dbContext) : base(dbContext)
    {
    }
}
