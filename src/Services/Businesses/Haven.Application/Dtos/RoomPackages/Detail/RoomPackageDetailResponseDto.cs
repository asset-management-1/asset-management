namespace Haven.Application.Dtos.RoomPackages.Detail;

/// <summary>
/// Represents one effective package in the context of a room.
/// </summary>
public sealed class RoomPackageDetailResponseDto
{
    /// <summary>
    /// Gets or sets the selected room context.
    /// </summary>
    public RoomPackageRoomResponseDto Room { get; set; }

    /// <summary>
    /// Gets or sets whether the room currently uses property-level common packages.
    /// </summary>
    public bool UsesCommonPackages { get; set; }

    /// <summary>
    /// Gets or sets the effective package detail.
    /// </summary>
    public RoomPackageTemplateResponseDto Package { get; set; }
}
