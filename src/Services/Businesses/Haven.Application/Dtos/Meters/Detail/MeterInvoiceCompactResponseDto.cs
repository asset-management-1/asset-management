namespace Haven.Application.Dtos.Meters.Detail;

/// <summary>
/// Identifies the invoice affected by a room meter save.
/// </summary>
public sealed class MeterInvoiceCompactResponseDto
{
    /// <summary>
    /// Gets or sets the public invoice identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice status code.
    /// </summary>
    public string StatusCode { get; set; }
}
