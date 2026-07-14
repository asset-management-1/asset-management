namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents the response returned after creating a property.
/// </summary>
public class CreatedPropertyResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe created property identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the generated property code.
    /// </summary>
    public string PropertyCode { get; set; }

    /// <summary>
    /// Gets or sets the created property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the persisted floor count.
    /// </summary>
    public int TotalFloors { get; set; }

    /// <summary>
    /// Gets or sets the persisted room count.
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Gets or sets the persisted floor summaries.
    /// </summary>
    public IReadOnlyList<CreatedPropertyFloorResponseDto> Floors { get; set; } = [];
}

