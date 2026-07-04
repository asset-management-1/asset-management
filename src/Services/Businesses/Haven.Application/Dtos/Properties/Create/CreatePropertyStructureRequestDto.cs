namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents the quick structure setup payload for building creation.
/// </summary>
public class CreatePropertyStructureRequestDto
{
    /// <summary>
    /// Gets or sets the number of floors to generate.
    /// </summary>
    public int TotalFloors { get; set; }

    /// <summary>
    /// Gets or sets the number of rooms to generate per floor.
    /// </summary>
    public int RoomsPerFloor { get; set; }

    /// <summary>
    /// Gets or sets the room numbering pattern, for example <c>{floor}{room}</c>.
    /// </summary>
    public string RoomNumberingPattern { get; set; } = DEFAULT_ROOM_NUMBERING_PATTERN;

    /// <summary>
    /// Gets or sets the default unit type code.
    /// </summary>
    public string DefaultUnitTypeCode { get; set; } = MASTER_CODE_UNIT_TYPE_ROOM;

    /// <summary>
    /// Gets or sets the default rental mode code.
    /// </summary>
    public string DefaultRentalModeCode { get; set; } = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT;

    /// <summary>
    /// Gets or sets default unit area.
    /// </summary>
    public decimal? DefaultAreaSqm { get; set; }

    /// <summary>
    /// Gets or sets default base rent.
    /// </summary>
    public decimal? DefaultBaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets default deposit.
    /// </summary>
    public decimal? DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the default total bed/slot quantity for shared-bed rooms.
    /// </summary>
    public int? DefaultTotalBeds { get; set; }

    /// <summary>
    /// Gets or sets whether pets are allowed by default.
    /// </summary>
    public bool DefaultIsPetAllowed { get; set; }

    /// <summary>
    /// Gets or sets explicit floor/room setup from the UI. When supplied, quick setup counts are ignored.
    /// </summary>
    public IReadOnlyList<CreatePropertyFloorRequestDto> Floors { get; set; } = [];
}

