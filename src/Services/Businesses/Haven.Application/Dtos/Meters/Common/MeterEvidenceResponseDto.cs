namespace Haven.Application.Dtos.Meters.Common;

/// <summary>
/// Describes one evidence file linked to a meter value.
/// </summary>
public sealed class MeterEvidenceResponseDto
{
    /// <summary>
    /// Gets or sets the evidence identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the public evidence file URL.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the stored evidence content type.
    /// </summary>
    public string ContentType { get; set; }
}
