namespace Haven.Application.Dtos.RoomPackages.List;

/// <summary>
/// Represents the effective package list for one room.
/// </summary>
public sealed class RoomPackageListResponseDto
{
    /// <summary>
    /// Gets or sets the selected room context.
    /// </summary>
    public RoomPackageRoomResponseDto Room { get; set; }

    /// <summary>
    /// Gets or sets whether the response currently falls back to property-level common packages.
    /// </summary>
    public bool UsesCommonPackages { get; set; }

    /// <summary>
    /// Gets or sets the effective package templates.
    /// </summary>
    public IReadOnlyList<RoomPackageTemplateResponseDto> Packages { get; set; } = [];
}
