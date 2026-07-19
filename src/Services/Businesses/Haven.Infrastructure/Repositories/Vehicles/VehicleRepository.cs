namespace Haven.Infrastructure.Repositories.Vehicles;

/// <summary>
/// Provides vehicle persistence and scoped lookup operations.
/// </summary>
public class VehicleRepository : GenericRepository<PartyVehicle>, IVehicleRepository
{
    private readonly HavenDbContext _havenDbContext;
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the vehicle repository.
    /// </summary>
    /// <param name="havenDbContext">The tracked Haven EF Core context.</param>
    /// <param name="dapperService">The Dapper service used for read projections.</param>
    public VehicleRepository(
        HavenDbContext havenDbContext,
        IDapperService dapperService)
        : base(havenDbContext)
    {
        _havenDbContext = havenDbContext;
        _dapperService = dapperService;
    }

    /// <summary>
    /// Loads every active vehicle inside a landlord-scoped room.
    /// </summary>
    /// <param name="parameters">The landlord and room scope.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The ordered compact vehicle rows.</returns>
    public async Task<IReadOnlyList<VehicleListRowModel>> GetByRoomAsync(
        VehicleListQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // The bounded room collection excludes image URLs because those fields belong only to vehicle detail.
        var rows = await _dapperService.QueryAsync<VehicleListRowModel>(
            InfrastructureQueryConstants.GET_VEHICLE_LIST_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Loads one vehicle detail row inside the current landlord scope.
    /// </summary>
    /// <param name="parameters">The public vehicle identifier and landlord scope.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The scoped vehicle detail row, or <c>null</c>.</returns>
    public Task<VehicleDetailRowModel> GetDetailAsync(
        VehicleScopeQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Detail joins master data for the public type label without expanding the write EF model.
        return _dapperService.QueryFirstOrDefaultAsync<VehicleDetailRowModel>(
            InfrastructureQueryConstants.GET_VEHICLE_DETAIL_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

    /// <summary>
    /// Loads one tracked vehicle in the current landlord property scope.
    /// </summary>
    /// <param name="parameters">The public vehicle identifier and landlord scope.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked vehicle entity, or <c>null</c>.</returns>
    public async Task<PartyVehicle> GetForMutationAsync(
        VehicleScopeQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Resolve only the authorised vehicle identifier through parameterised Dapper SQL.
        var vehicleId = await _dapperService.ExecuteScalarAsync<long?>(
            InfrastructureQueryConstants.GET_SCOPED_VEHICLE_ID_QUERY,
            new
            {
                parameters.VehiclePublicId,
                parameters.RoomPublicId,
                parameters.CurrentPartyId,
                RelationshipCodes = parameters.RelationshipCodes.ToArray()
            },
            DapperCommandOptionsHelper.CreateText(cancellationToken));
        if (!vehicleId.HasValue)
        {
            return null;
        }

        // Step 2: Materialise the scoped vehicle through EF so the caller receives tracked mutation state.
        return await _havenDbContext.PartyVehicles
            .FirstOrDefaultAsync(vehicle => vehicle.Id == vehicleId.Value, cancellationToken);
    }

    /// <summary>
    /// Resolves a primary tenant with an active rental contract in one room.
    /// </summary>
    /// <param name="parameters">The room, payer, active-status, and landlord-scope inputs.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The eligible payer row, or <c>null</c>.</returns>
    public Task<VehiclePayerRowModel> GetPrimaryPayerAsync(
        VehiclePayerEligibilityQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // The Dapper projection applies the same active relationship scope used by vehicle list and detail reads.
        return _dapperService.QueryFirstOrDefaultAsync<VehiclePayerRowModel>(
            InfrastructureQueryConstants.GET_VEHICLE_PRIMARY_PAYER_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

}
