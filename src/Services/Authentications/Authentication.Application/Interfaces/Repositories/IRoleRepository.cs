using Authentication.Domain.Entities;

namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines role-specific repository operations.
/// </summary>
public interface IRoleRepository : IGenericRepository<Role>
{
    /// <summary>
    /// Loads the role associated with a party type.
    /// </summary>
    Task<Role> GetByPartyTypeAsync(string partyType, CancellationToken cancellationToken = default);
}
