namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents property structure data used by building detail and edit screens.
/// </summary>
public class PropertyDetailStructureResponseDto
{
    /// <summary>
    /// Gets or sets the total number of floors derived from stored property/unit data.
    /// </summary>
    public int TotalFloors { get; set; }

    /// <summary>
    /// Gets or sets the total number of rooms derived from stored property/unit data.
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Gets or sets floors and edit-summary rooms grouped by Units.FloorNumber.
    /// </summary>
    public IReadOnlyList<PropertyDetailFloorResponseDto> Floors { get; set; } = [];
}
