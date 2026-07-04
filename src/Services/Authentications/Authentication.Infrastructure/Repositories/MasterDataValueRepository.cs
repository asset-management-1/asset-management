namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access methods for master data values.
/// </summary>
public class MasterDataValueRepository : GenericRepository<MasterDataValue>, IMasterDataValueRepository
{
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the master-data repository with EF writes and Dapper batch lookup support.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    /// <param name="dapperService">The Dapper service used for batched lookup reads.</param>
    public MasterDataValueRepository(
        AuthenticationDbContext dbContext,
        IDapperService dapperService) : base(dbContext)
    {
        _dapperService = dapperService;
    }

    /// <summary>
    /// Gets an active master data value by master data type and value.
    /// </summary>
    /// <param name="type">The master data type code or name.</param>
    /// <param name="value">The master data value code or name.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched active master data value; otherwise <c>null</c>.</returns>
    public async Task<MasterDataValue> GetByTypeAndValueAsync(
        string type,
        string value,
        CancellationToken cancellationToken = default)
    {
        // Missing lookup parts cannot resolve to an active master-data value.
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Delegate through the batch lookup so single and multi-value flows share one SQL path.
        var key = new MasterDataValueKeyModel(type, value);
        var masterDataValues = await GetByTypeAndValuesAsync([key], cancellationToken);

        return masterDataValues.TryGetValue(key, out var masterDataValue)
            ? masterDataValue
            : null;
    }

    /// <summary>
    /// Gets active master data values by many master-data type/value pairs in one Dapper query.
    /// </summary>
    /// <param name="keys">The master-data keys to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The resolved master data values keyed by requested type/value.</returns>
    public async Task<IReadOnlyDictionary<MasterDataValueKeyModel, MasterDataValue>> GetByTypeAndValuesAsync(
        IReadOnlyCollection<MasterDataValueKeyModel> keys,
        CancellationToken cancellationToken = default)
    {
        // Remove invalid and duplicate keys before building the Dapper request arrays.
        var requestedKeys = (keys ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Type) && !string.IsNullOrWhiteSpace(x.Value))
            .Distinct()
            .ToList();
        if (requestedKeys.Count == 0)
        {
            return new Dictionary<MasterDataValueKeyModel, MasterDataValue>();
        }

        // Pass parallel arrays so PostgreSQL can unnest the request set and resolve all master data at once.
        var rows = await _dapperService.QueryAsync<MasterDataValueRowModel>(
            InfrastructureQueryConstants.GET_MASTER_DATA_VALUES_BY_TYPE_AND_VALUE_QUERY,
            new
            {
                KeyTypes = requestedKeys.Select(x => x.Type).ToArray(),
                KeyValues = requestedKeys.Select(x => x.Value).ToArray(),
                KeyOrdinals = Enumerable.Range(1, requestedKeys.Count).ToArray()
            },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToDictionary(
            x => new MasterDataValueKeyModel(x.KeyType, x.KeyValue),
            MapRowToMasterDataValue);
    }

    /// <summary>
    /// Maps the minimal Dapper lookup row into the master-data value shape expected by existing flows.
    /// </summary>
    /// <param name="row">The row returned by the batched master-data lookup query.</param>
    /// <returns>The master-data value entity with only lookup-required fields populated.</returns>
    private static MasterDataValue MapRowToMasterDataValue(MasterDataValueRowModel row)
    {
        // Populate only the master-data columns required by callers after lookup resolution.
        return new MasterDataValue
        {
            Id = row.Id,
            MasterDataTypeId = row.MasterDataTypeId,
            Code = row.Code,
            Name = row.Name
        };
    }
}
