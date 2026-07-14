namespace Haven.Application.Dtos.Rooms.Detail;

/// <summary>
/// Represents the room detail response.
/// </summary>
public class RoomDetailResponseDto
{
    /// <summary>
    /// Gets or sets the editable room basic information.
    /// </summary>
    public RoomDetailBasicInfoResponseDto BasicInfo { get; set; }

    /// <summary>
    /// Gets or sets effective charge policies for the room.
    /// </summary>
    public IReadOnlyList<RoomChargePolicyResponseDto> ChargePolicies { get; set; } = [];
}
