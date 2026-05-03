namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides user-role-specific data access over the authentication database context.
/// </summary>
public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public UserRoleRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
    }
}
