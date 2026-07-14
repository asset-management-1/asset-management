namespace Haven.Application.Models.Meters.Rows;

/// <summary>
/// Projects evidence metadata linked to one meter record.
/// </summary>
public sealed class MeterEvidenceRowModel
{
    /// <summary>
    /// Gets or sets the frontend-safe evidence identifier.
    /// </summary>
    public Guid EvidencePublicId { get; set; }

    /// <summary>
    /// Gets or sets the internal meter record identifier.
    /// </summary>
    public long MeterId { get; set; }

    /// <summary>
    /// Gets or sets the public evidence URL.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the stored content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the evidence order inside its meter slot.
    /// </summary>
    public int SortOrder { get; set; }

}
