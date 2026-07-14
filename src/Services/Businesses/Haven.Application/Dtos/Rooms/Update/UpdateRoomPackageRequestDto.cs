namespace Haven.Application.Dtos.Rooms.Update;

/// <summary>
/// Represents one room-level package template submitted by the room edit form.
/// </summary>
public class UpdateRoomPackageRequestDto
{
    /// <summary>
    /// Gets or sets the existing package identifier. New packages omit this value.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the package display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the package price adjustment.
    /// </summary>
    public decimal? PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets package item labels.
    /// </summary>
    public IReadOnlyList<UpdateRoomPackageItemRequestDto> Items { get; set; }
}
