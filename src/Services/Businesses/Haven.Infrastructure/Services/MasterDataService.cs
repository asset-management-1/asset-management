namespace Haven.Infrastructure.Services;

/// <summary>
/// Resolves active master-data values for business workflows.
/// </summary>
public class MasterDataService : IMasterDataService
{
    private readonly IMasterDataRepository _masterDataRepository;
    private readonly ILogger<MasterDataService> _logger;

    /// <summary>
    /// Creates the master-data service.
    /// </summary>
    /// <param name="masterDataRepository">The master-data repository.</param>
    /// <param name="logger">The service logger.</param>
    public MasterDataService(
        IMasterDataRepository masterDataRepository,
        ILogger<MasterDataService> logger)
    {
        _masterDataRepository = masterDataRepository;
        _logger = logger;
    }

    /// <summary>
    /// Loads active master-data values by type/code pairs.
    /// </summary>
    /// <param name="keys">The master-data keys to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>Resolved values keyed by requested key.</returns>
    public async Task<IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel>> GetValuesAsync(
        IReadOnlyCollection<MasterDataKeyModel> keys,
        CancellationToken cancellationToken = default)
    {
        // Master-data callers pass the exact type/code keys needed by the use case.
        var requestedKeys = keys ?? Array.Empty<MasterDataKeyModel>();
        var values = await _masterDataRepository.GetByTypeAndCodesAsync(requestedKeys, cancellationToken);

        // Validate every requested key once so downstream mapping can rely on complete values.
        foreach (var key in requestedKeys)
        {
            if (!values.ContainsKey(key))
            {
                _logger.LogWarning(
                    InfrastructureLogConstants.MasterDataLogs.MASTER_DATA_VALUE_MISSING,
                    key.Type,
                    key.Code);
            }

            values.RequireValue(key.Type, key.Code);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.MasterDataLogs.MASTER_DATA_VALUES_VALIDATED,
            requestedKeys.Count);

        return values;
    }
}
