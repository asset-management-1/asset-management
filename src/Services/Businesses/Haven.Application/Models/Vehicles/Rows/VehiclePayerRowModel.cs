namespace Haven.Application.Models.Vehicles.Rows;

/// <summary>
/// Represents the resolved room and eligible primary tenant payer identifiers.
/// </summary>
public sealed class VehiclePayerRowModel
{
    /// <summary>
    /// Gets or sets the internal room identifier.
    /// </summary>
    public long UnitId { get; set; }

    /// <summary>
    /// Gets or sets the internal payer party identifier.
    /// </summary>
    public long PartyId { get; set; }
}
