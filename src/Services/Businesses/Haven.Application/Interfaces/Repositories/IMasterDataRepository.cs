namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides active master-data read operations.
/// </summary>
public interface IMasterDataRepository
{
    /// <summary>
    /// Loads active master-data values by type/code pairs.
    /// </summary>
    /// <param name="keys">The master-data keys to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>Resolved values keyed by requested key.</returns>
    Task<IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel>> GetByTypeAndCodesAsync(
        IReadOnlyCollection<MasterDataKeyModel> keys,
        CancellationToken cancellationToken = default);
}
