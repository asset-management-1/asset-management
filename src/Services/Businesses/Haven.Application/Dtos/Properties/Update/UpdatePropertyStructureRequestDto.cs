namespace Haven.Application.Dtos.Properties.Update;

/// <summary>
/// Represents the full editable room structure submitted by the property edit form.
/// </summary>
public class UpdatePropertyStructureRequestDto
{
    /// <summary>
    /// Gets or sets the edited floor list.
    /// </summary>
    public IReadOnlyList<UpdatePropertyFloorRequestDto> Floors { get; set; } = [];
}
