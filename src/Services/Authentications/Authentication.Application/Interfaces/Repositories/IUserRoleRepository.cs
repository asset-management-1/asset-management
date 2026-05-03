using Authentication.Domain.Entities;

namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines user-role-specific repository operations.
/// </summary>
public interface IUserRoleRepository : IGenericRepository<UserRole>
{
}
