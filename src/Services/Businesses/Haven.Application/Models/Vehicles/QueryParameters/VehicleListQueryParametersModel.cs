namespace Haven.Application.Models.Vehicles.QueryParameters;

/// <summary>
/// Represents Dapper parameters for one room-scoped vehicle collection.
/// </summary>
public sealed class VehicleListQueryParametersModel
{
    /// <summary>
    /// Gets or sets the internal current landlord party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets relationship codes that grant property access.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the required frontend-safe room identifier.
    /// </summary>
    public Guid RoomId { get; set; }
}
