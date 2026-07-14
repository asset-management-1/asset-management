namespace Haven.Application.Dtos.Meters.Detail;

/// <summary>
/// Describes the live invoice action the backend will take when the meter period is saved.
/// </summary>
public sealed class MeterInvoiceImpactResponseDto
{
    /// <summary>
    /// Gets or sets the action the backend will perform when the period is saved.
    /// </summary>
    public string ActionCode { get; set; }

    /// <summary>
    /// Gets or sets whether the live invoice state currently allows saving.
    /// </summary>
    public bool CanSave { get; set; }

    /// <summary>
    /// Gets or sets the stable reason code when saving is blocked.
    /// </summary>
    public string BlockedReasonCode { get; set; }

    /// <summary>
    /// Gets or sets the affected invoice when one already exists.
    /// </summary>
    public MeterInvoiceCompactResponseDto Invoice { get; set; }
}
