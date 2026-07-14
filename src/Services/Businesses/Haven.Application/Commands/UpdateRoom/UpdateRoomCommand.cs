namespace Haven.Application.Commands.UpdateRoom;

/// <summary>
/// Represents a request to update one room edit form.
/// </summary>
public class UpdateRoomCommand : ICommand<ResponseDto<RoomDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid Id { get; set; }

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
    /// Gets or sets the charge policy mode when the charge-policy section is submitted.
    /// </summary>
    public string ChargePolicyMode { get; set; }

    /// <summary>
    /// Gets or sets room-level charge policies when custom mode is selected.
    /// </summary>
    public IReadOnlyList<UpdateRoomChargePolicyRequestDto> ChargePolicies { get; set; }

    /// <summary>
    /// Gets or sets the package mode when the package section is submitted.
    /// </summary>
    public string PackageMode { get; set; }

    /// <summary>
    /// Gets or sets room-level package templates when custom mode is selected.
    /// </summary>
    public IReadOnlyList<UpdateRoomPackageRequestDto> Packages { get; set; }
}
