namespace Haven.Infrastructure.Services.MasterData;

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
        // Accept an empty request so callers do not need a separate no-lookup branch.
        var requestedKeys = keys ?? [];

        // Load the exact active values required by this use case in one repository query.
        var values = await _masterDataRepository.GetByTypeAndCodesAsync(requestedKeys, cancellationToken);

        // Find the first unresolved key, so downstream mapping can rely on complete values.
        var missingKey = requestedKeys
            .Where(key => !values.ContainsKey(key))
            .FirstOrDefault();

        if (missingKey is not null)
        {
            // Log the failed public type/code pair before returning the standard client-safe lookup error.
            _logger.LogWarning(
                InfrastructureLogConstants.MasterDataLogs.MASTER_DATA_VALUE_MISSING,
                missingKey.Type,
                missingKey.Code);

            throw new ApiException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    ApplicationErrorConstants.LookupErrors.ERROR_MASTER_DATA_NOT_FOUND,
                    missingKey.Type,
                    missingKey.Code),
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }

        // Record the validated lookup count without logging caller payload values.
        _logger.LogInformation(
            InfrastructureLogConstants.MasterDataLogs.MASTER_DATA_VALUES_VALIDATED,
            requestedKeys.Count);

        return values;
    }
}
