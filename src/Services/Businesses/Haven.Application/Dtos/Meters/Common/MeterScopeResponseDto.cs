namespace Haven.Application.Dtos.Meters.Common;

/// <summary>
/// Identifies a property or room displayed by the meter screens.
/// </summary>
public sealed class MeterScopeResponseDto
{
    /// <summary>
    /// Gets or sets the public identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; }
}
