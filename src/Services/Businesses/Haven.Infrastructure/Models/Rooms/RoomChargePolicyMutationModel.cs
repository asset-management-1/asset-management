namespace Haven.Infrastructure.Models.Rooms;

/// <summary>
/// Represents one room charge-policy edit with its resolved persistence identifiers.
/// </summary>
public sealed class RoomChargePolicyMutationModel
{
    /// <summary>
    /// Gets or sets the submitted room charge-policy row.
    /// </summary>
    public UpdateRoomChargePolicyRequestDto Policy { get; set; }

    /// <summary>
    /// Gets or sets the master-data identifiers resolved for the submitted row.
    /// </summary>
    public RentalChargePolicyLookupModel Lookups { get; set; }
}
