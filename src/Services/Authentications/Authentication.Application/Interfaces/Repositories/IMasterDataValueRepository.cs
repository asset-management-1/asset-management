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

    /// <summary>
    /// Loads multiple active master data values by type/value pairs in one database round-trip.
    /// </summary>
    /// <param name="lookups">The master-data type/value pairs to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The resolved master-data values keyed by requested lookup type/value.</returns>
    Task<IReadOnlyDictionary<MasterDataValueLookupModel, MasterDataValue>> GetByTypeAndValuesAsync(
        IReadOnlyCollection<MasterDataValueLookupModel> lookups,
        CancellationToken cancellationToken = default);
}
