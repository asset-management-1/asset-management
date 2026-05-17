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
        var lookup = new MasterDataValueLookupModel(type, value);
        var masterDataValues = await GetByTypeAndValuesAsync([lookup], cancellationToken);

        return masterDataValues.TryGetValue(lookup, out var masterDataValue)
            ? masterDataValue
            : null;
    }

    /// <summary>
    /// Gets active master data values by many master-data type/value pairs in one Dapper query.
    /// </summary>
    /// <param name="lookups">The master-data lookup pairs to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The resolved master data values keyed by requested lookup type/value.</returns>
    public async Task<IReadOnlyDictionary<MasterDataValueLookupModel, MasterDataValue>> GetByTypeAndValuesAsync(
        IReadOnlyCollection<MasterDataValueLookupModel> lookups,
        CancellationToken cancellationToken = default)
    {
        // Remove invalid and duplicate lookup pairs before building the Dapper request arrays.
        var requestedLookups = (lookups ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Type) && !string.IsNullOrWhiteSpace(x.Value))
            .Distinct()
            .ToList();
        if (requestedLookups.Count == 0)
        {
            return new Dictionary<MasterDataValueLookupModel, MasterDataValue>();
        }

        // Keep command options local so the caller's cancellation token flows into Dapper.
        var queryOptions = new DapperCommandOptions
        {
            CommandType = CommandType.Text,
            CancellationToken = cancellationToken
        };

        // Pass parallel arrays so PostgreSQL can unnest the request set and resolve all master data at once.
        var rows = await _dapperService.QueryAsync<MasterDataValueLookupRowModel>(
            InfrastructureQueryConstants.GET_MASTER_DATA_VALUES_BY_TYPE_AND_VALUE_QUERY,
            new
            {
                LookupTypes = requestedLookups.Select(x => x.Type).ToArray(),
                LookupValues = requestedLookups.Select(x => x.Value).ToArray(),
                LookupOrdinals = Enumerable.Range(1, requestedLookups.Count).ToArray()
            },
            queryOptions);

        return rows.ToDictionary(
            x => new MasterDataValueLookupModel(x.LookupType, x.LookupValue),
            MapRowToMasterDataValue);
    }

    /// <summary>
    /// Maps the minimal Dapper lookup row into the master-data value shape expected by existing flows.
    /// </summary>
    /// <param name="row">The row returned by the batched master-data lookup query.</param>
    /// <returns>The master-data value entity with only lookup-required fields populated.</returns>
    private static MasterDataValue MapRowToMasterDataValue(MasterDataValueLookupRowModel row)
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
