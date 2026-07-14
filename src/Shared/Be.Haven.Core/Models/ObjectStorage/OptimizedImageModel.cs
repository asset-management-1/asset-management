namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents the canonical JPEG content produced from one submitted image.
/// </summary>
public class OptimizedImageModel
{
    /// <summary>
    /// Gets or sets the optimized JPEG bytes.
    /// </summary>
    public byte[] Content { get; set; }

    /// <summary>
    /// Gets or sets the canonical content type used when the image is uploaded.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the canonical file name used to derive the object-key extension.
    /// </summary>
    public string FileName { get; set; }
}
