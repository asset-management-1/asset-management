namespace Haven.Application.Dtos.Properties.List;

/// <summary>
/// Represents one property item in the landlord property list.
/// </summary>
public class PropertyListItemResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe property identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the single-line address shown on the property card.
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail URL when a property image exists.
    /// </summary>
    public string ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the total number of floors shown on the property card.
    /// </summary>
    public int TotalFloors { get; set; }

    /// <summary>
    /// Gets or sets the total number of rooms shown on the property card.
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Gets or sets the available room count shown on the property card.
    /// </summary>
    public int AvailableRooms { get; set; }

    /// <summary>
    /// Gets or sets the occupancy rate as a decimal from 0 to 1.
    /// </summary>
    public decimal? OccupancyRate { get; set; }

    /// <summary>
    /// Gets or sets floor groups for the current UI.
    /// </summary>
    public IReadOnlyList<PropertyListFloorResponseDto> Floors { get; set; } = [];
}

