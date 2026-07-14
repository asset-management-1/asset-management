namespace Haven.Application.Dtos.Properties.Create;

/// <summary>
/// Represents the final floor and room structure sent by the property setup UI.
/// </summary>
public class CreatePropertyStructureRequestDto
{
    /// <summary>
    /// Gets or sets the FE-composed floors and rooms to persist.
    /// </summary>
    public IReadOnlyList<CreatePropertyFloorRequestDto> Floors { get; set; } = [];
}

