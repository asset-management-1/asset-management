namespace Haven.Application.Dtos.RoomPackages.Common;

/// <summary>
/// Represents the compact room context returned by room-package APIs.
/// </summary>
public sealed class RoomPackageRoomResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string Name { get; set; }
}
