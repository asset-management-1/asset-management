namespace Haven.Application.Models.Vehicles.QueryParameters;

/// <summary>
/// Represents landlord scope for one vehicle detail or tracked mutation read.
/// </summary>
public sealed class VehicleScopeQueryParametersModel
{
    /// <summary>
    /// Gets or sets the frontend-safe owning room identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe vehicle identifier.
    /// </summary>
    public Guid VehiclePublicId { get; set; }

    /// <summary>
    /// Gets or sets the current landlord party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets relationship codes that grant access to the vehicle's property.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];
}
