using Authentication.Domain.Entities;

namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines master-data-value-specific repository operations.
/// </summary>
public interface IMasterDataValueRepository : IGenericRepository<MasterDataValue>
{
    /// <summary>
    /// Loads a master data value by type and value.
    /// </summary>
    Task<MasterDataValue> GetByTypeAndValueAsync(
        string type,
        string value,
        CancellationToken cancellationToken = default);
}
