namespace Authentication.Infrastructure.Models.Users.Vehicles;

/// <summary>
/// Groups resolved data required to register one tenant profile vehicle.
/// </summary>
internal sealed class VehicleRegistrationContextModel
{
    /// <summary>
    /// Gets or sets the active tenant party identifier that owns the vehicle.
    /// </summary>
    public long TenantPartyId { get; set; }

    /// <summary>
    /// Gets or sets the resolved vehicle type master-data value.
    /// </summary>
    public MasterDataValue VehicleType { get; set; }
}
