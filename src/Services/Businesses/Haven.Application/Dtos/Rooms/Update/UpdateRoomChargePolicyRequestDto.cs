namespace Haven.Application.Dtos.Rooms.Update;

/// <summary>
/// Represents one room-level charge policy submitted by the room edit form.
/// </summary>
public class UpdateRoomChargePolicyRequestDto : IChargePolicyInput
{
    /// <summary>
    /// Gets or sets the frontend-safe policy identifier for existing room-level policies.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice line type code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the charge display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the charge amount.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Gets or sets the calculation method code.
    /// </summary>
    public string CalculationMethodCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type code for parking policies.
    /// </summary>
    public string VehicleTypeCode { get; set; }
}
