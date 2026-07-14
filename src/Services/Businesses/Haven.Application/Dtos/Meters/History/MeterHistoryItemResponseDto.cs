namespace Haven.Application.Dtos.Meters.History;

/// <summary>
/// Returns one calendar month in a room's meter history.
/// </summary>
public sealed class MeterHistoryItemResponseDto
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
    /// Gets or sets the latest recorded date in the month.
    /// </summary>
    public DateOnly? Date { get; set; }

    /// <summary>
    /// Gets or sets the electricity meter value.
    /// </summary>
    public MeterValueResponseDto Electric { get; set; }

    /// <summary>
    /// Gets or sets the water meter value.
    /// </summary>
    public MeterValueResponseDto Water { get; set; }

    /// <summary>
    /// Gets or sets whether the period can be edited in its current invoice state.
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets the stable reason code when editing is blocked.
    /// </summary>
    public string EditBlockedReasonCode { get; set; }
}
