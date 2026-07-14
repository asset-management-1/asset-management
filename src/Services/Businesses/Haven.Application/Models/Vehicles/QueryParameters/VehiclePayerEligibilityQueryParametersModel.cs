namespace Haven.Application.Models.Vehicles.QueryParameters;

/// <summary>
/// Represents the scoped contract and occupancy criteria for an eligible vehicle payer.
/// </summary>
public sealed class VehiclePayerEligibilityQueryParametersModel
{
    /// <summary>
    /// Gets or sets the optional frontend-safe room identifier for vehicle creation.
    /// </summary>
    public Guid? RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the optional internal room identifier for vehicle reassignment.
    /// </summary>
    public long? UnitId { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe primary tenant identifier.
    /// </summary>
    public Guid PayerTenantPublicId { get; set; }

    /// <summary>
    /// Gets or sets the current landlord party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets the active occupancy-status identifier.
    /// </summary>
    public long ActiveOccupancyStatusId { get; set; }

    /// <summary>
    /// Gets or sets the active contract-status identifier.
    /// </summary>
    public long ActiveContractStatusId { get; set; }

    /// <summary>
    /// Gets or sets relationship codes that grant access to the payer's room property.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];
}
