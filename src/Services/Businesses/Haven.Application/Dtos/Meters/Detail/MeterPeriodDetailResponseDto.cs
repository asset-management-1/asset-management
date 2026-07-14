namespace Haven.Application.Dtos.Meters.Detail;

/// <summary>
/// Returns one selected room meter period and its immediately preceding period.
/// </summary>
public sealed class MeterPeriodDetailResponseDto
{
    /// <summary>
    /// Gets or sets the property containing the room.
    /// </summary>
    public MeterScopeResponseDto Property { get; set; }

    /// <summary>
    /// Gets or sets the selected room.
    /// </summary>
    public MeterScopeResponseDto Room { get; set; }

    /// <summary>
    /// Gets or sets the selected meter period.
    /// </summary>
    public MeterPeriodResponseDto CurrentPeriod { get; set; }

    /// <summary>
    /// Gets or sets the immediately preceding meter period.
    /// </summary>
    public MeterPeriodResponseDto PreviousPeriod { get; set; }

    /// <summary>
    /// Gets or sets the current invoice impact used by the UI for warning display.
    /// </summary>
    public MeterInvoiceImpactResponseDto InvoiceImpact { get; set; }
}
