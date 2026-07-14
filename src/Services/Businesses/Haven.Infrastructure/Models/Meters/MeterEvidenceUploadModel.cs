namespace Haven.Infrastructure.Models.Meters;

/// <summary>
/// Carries one uploaded evidence object together with its original form metadata.
/// </summary>
public sealed class MeterEvidenceUploadModel
{
    /// <summary>
    /// Gets or sets the original multipart file.
    /// </summary>
    public IFormFile File { get; set; }

    /// <summary>
    /// Gets or sets the uploaded object metadata returned by storage.
    /// </summary>
    public ObjectUploadResponseModel Upload { get; set; }

    /// <summary>
    /// Gets or sets the configured public URL for the uploaded evidence object.
    /// </summary>
    public string PublicUrl { get; set; }

    /// <summary>
    /// Gets or sets the electricity or water type code for the owning meter record.
    /// </summary>
    public string UtilityTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the image order supplied within its electricity or water collection.
    /// </summary>
    public int SubmittedOrder { get; set; }
}
