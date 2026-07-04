namespace Haven.Infrastructure.Repositories;

/// <summary>
/// Provides master-data persistence reads.
/// </summary>
public class MasterDataRepository : IMasterDataRepository
{
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the master-data repository.
    /// </summary>
    /// <param name="dapperService">The shared Dapper service.</param>
    public MasterDataRepository(IDapperService dapperService)
    {
        _dapperService = dapperService;
    }

    /// <summary>
    /// Loads active master-data values by type/code pairs.
    /// </summary>
    /// <param name="keys">The master-data keys to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>Resolved values keyed by requested key.</returns>
    public async Task<IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel>> GetByTypeAndCodesAsync(
        IReadOnlyCollection<MasterDataKeyModel> keys,
        CancellationToken cancellationToken = default)
    {
        var requestedKeys = (keys ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Type) && !string.IsNullOrWhiteSpace(x.Code))
            .Distinct()
            .ToList();

        if (requestedKeys.Count == 0)
        {
            return new Dictionary<MasterDataKeyModel, MasterDataValueModel>();
        }

        var rows = await _dapperService.QueryAsync<MasterDataRowModel>(
            InfrastructureQueryConstants.GET_MASTER_DATA_VALUES_BY_TYPE_AND_CODE_QUERY,
            new
            {
                KeyTypes = requestedKeys.Select(x => x.Type).ToArray(),
                KeyCodes = requestedKeys.Select(x => x.Code).ToArray(),
                KeyOrdinals = Enumerable.Range(1, requestedKeys.Count).ToArray()
            },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToDictionary(
            x => new MasterDataKeyModel(x.KeyType, x.KeyCode),
            x => x.Adapt<MasterDataValueModel>());
    }
}
