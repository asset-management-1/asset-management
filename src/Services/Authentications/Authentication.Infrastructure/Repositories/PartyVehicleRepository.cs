namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// Provides data access operations for tenant profile vehicles.
/// </summary>
public class PartyVehicleRepository : GenericRepository<PartyVehicle>, IPartyVehicleRepository
{
    private readonly AuthenticationDbContext _authenticationDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyVehicleRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The authentication database context.</param>
    public PartyVehicleRepository(AuthenticationDbContext dbContext) : base(dbContext)
    {
        _authenticationDbContext = dbContext;
    }

    /// <summary>
    /// Loads active vehicles for one tenant party.
    /// </summary>
    /// <param name="partyId">The tenant party internal identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The active profile vehicles registered under the tenant party.</returns>
    public async Task<IReadOnlyList<UserVehicleResponseDto>> GetActiveByPartyIdAsync(
        long partyId,
        CancellationToken cancellationToken = default)
    {
        // Project only raw fields needed for the vehicle response and map code values after materialization.
        var vehicles = await _authenticationDbContext.PartyVehicles
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PartyId == partyId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Select(x => new UserVehicleReadModel
            {
                PublicId = x.PublicId,
                VehicleType = x.VehicleType.Code != null && x.VehicleType.Code != string.Empty
                    ? x.VehicleType.Code
                    : x.VehicleType.Name,
                VehicleTypeDisplayName = x.VehicleType.Name,
                VehicleName = x.VehicleName,
                LicensePlate = x.LicensePlate,
                Status = nameof(UserVehicleStatusEnum.Active),
                ThumbnailUrl = x.FrontImageUrl ?? x.SideImageUrl,
                FrontImageUrl = x.FrontImageUrl,
                SideImageUrl = x.SideImageUrl
            })
            .ToListAsync(cancellationToken);

        return vehicles.Adapt<List<UserVehicleResponseDto>>();
    }

    /// <summary>
    /// Loads a tracked vehicle by public id while enforcing tenant-party ownership.
    /// </summary>
    /// <param name="vehiclePublicId">The public vehicle identifier supplied by the API.</param>
    /// <param name="partyId">The tenant party internal identifier that must own the vehicle.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The tracked active vehicle when found; otherwise <c>null</c>.</returns>
    public Task<PartyVehicle> GetTrackedByPublicIdAndPartyIdAsync(
        Guid vehiclePublicId,
        long partyId,
        CancellationToken cancellationToken = default)
    {
        // Return a tracked row only when the active tenant party owns the vehicle.
        return _authenticationDbContext.PartyVehicles
            .FirstOrDefaultAsync(
                x => !x.IsDeleted
                     && x.PublicId == vehiclePublicId
                     && x.PartyId == partyId,
                cancellationToken);
    }
}
