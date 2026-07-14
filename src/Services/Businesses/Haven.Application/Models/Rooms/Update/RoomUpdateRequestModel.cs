namespace Haven.Application.Models.Rooms.Update;

/// <summary>
/// Represents a party-scoped room update request.
/// </summary>
public class RoomUpdateRequestModel
{
    /// <summary>
    /// Gets or sets the current party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the room public identifier from the body.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the room/unit type code.
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// Gets or sets the rental mode code.
    /// </summary>
    public string RentalModeCode { get; set; }

    /// <summary>
    /// Gets or sets the room area in square meters.
    /// </summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>
    /// Gets or sets the base rent amount.
    /// </summary>
    public decimal? BaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the default deposit amount.
    /// </summary>
    public decimal? DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets total bed capacity for shared-bed/KTX rooms.
    /// </summary>
    public int? TotalBeds { get; set; }

    /// <summary>
    /// Gets or sets whether pets are allowed when the field is submitted.
    /// </summary>
    public bool? IsPetAllowed { get; set; }

    /// <summary>
    /// Gets or sets how charge policies should be handled when that section is submitted.
    /// </summary>
    public string ChargePolicyMode { get; set; }

    /// <summary>
    /// Gets or sets room-level charge policies when custom mode is selected.
    /// </summary>
    public IReadOnlyList<UpdateRoomChargePolicyRequestDto> ChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets how package templates should be handled when that section is submitted.
    /// </summary>
    public string PackageMode { get; set; }

    /// <summary>
    /// Gets or sets room-level packages when custom mode is selected.
    /// </summary>
    public IReadOnlyList<UpdateRoomPackageRequestDto> Packages { get; set; }
}
