namespace Haven.Application.Dtos.Meters.Common;

/// <summary>
/// Returns one electricity or water meter value with backend-calculated usage and amount.
/// </summary>
public sealed class MeterValueResponseDto
{
    /// <summary>
    /// Gets or sets the public meter record identifier.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the previous meter value used as the calculation baseline.
    /// </summary>
    public decimal? Previous { get; set; }

    /// <summary>
    /// Gets or sets the current meter value.
    /// </summary>
    public decimal? Current { get; set; }

    /// <summary>
    /// Gets or sets the backend-calculated consumption.
    /// </summary>
    public decimal? UsageQuantity { get; set; }

    /// <summary>
    /// Gets or sets the price suggested by the effective room or property policy.
    /// </summary>
    public decimal? SuggestedPrice { get; set; }

    /// <summary>
    /// Gets or sets the price snapshot confirmed for the period.
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Gets or sets the backend-calculated charge amount.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Gets or sets the date on which the meter value was recorded.
    /// </summary>
    public DateOnly? Date { get; set; }

    /// <summary>
    /// Gets or sets the meter status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the effective charge calculation method code.
    /// </summary>
    public string CalculationMethodCode { get; set; }

    /// <summary>
    /// Gets or sets whether the previous value came from a confirmed period or manual baseline.
    /// </summary>
    public string PreviousSourceCode { get; set; }

    /// <summary>
    /// Gets or sets evidence images linked to this meter value in display order.
    /// </summary>
    public IReadOnlyList<MeterEvidenceResponseDto> Evidence { get; set; } = [];
}
