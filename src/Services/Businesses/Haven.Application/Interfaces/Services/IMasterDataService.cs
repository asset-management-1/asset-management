namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Resolves active master-data values for Haven business workflows.
/// </summary>
public interface IMasterDataService
{
    /// <summary>
    /// Loads active master-data values by type/code pairs.
    /// </summary>
    /// <param name="keys">The master-data keys to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>Resolved values keyed by requested key.</returns>
    Task<IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel>> GetValuesAsync(
        IReadOnlyCollection<MasterDataKeyModel> keys,
        CancellationToken cancellationToken = default);
}
