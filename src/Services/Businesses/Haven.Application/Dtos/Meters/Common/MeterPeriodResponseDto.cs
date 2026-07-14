namespace Haven.Application.Dtos.Meters.Common;

/// <summary>
/// Groups electricity and water meter values for one calendar month.
/// </summary>
public sealed class MeterPeriodResponseDto
{
    /// <summary>
    /// Gets or sets the calendar month.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the calendar year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the electricity meter value when the room uses a meter policy.
    /// </summary>
    public MeterValueResponseDto Electric { get; set; }

    /// <summary>
    /// Gets or sets the water meter value when the room uses a meter policy.
    /// </summary>
    public MeterValueResponseDto Water { get; set; }
}
