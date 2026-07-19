namespace Haven.Domain.Entities;

/// <summary>
/// Represents object-storage metadata for a utility evidence file.
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// Gets or sets the document-type master-data identifier.
    /// </summary>
    public long DocumentTypeId { get; set; }

    /// <summary>
    /// Gets or sets the generated storage file name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the original client file name.
    /// </summary>
    public string OriginalFileName { get; set; }

    /// <summary>
    /// Gets or sets the detected file content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the normalized file extension.
    /// </summary>
    public string FileExtension { get; set; }

    /// <summary>
    /// Gets or sets the stored file size in bytes.
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Gets or sets the storage-provider master-data identifier.
    /// </summary>
    public long StorageProviderId { get; set; }

    /// <summary>
    /// Gets or sets the private object-storage path.
    /// </summary>
    public string StoragePath { get; set; }

    /// <summary>
    /// Gets or sets the public or signed file URL when available.
    /// </summary>
    public string FileUrl { get; set; }

    /// <summary>
    /// Gets or sets the file-integrity checksum.
    /// </summary>
    public string Checksum { get; set; }

    /// <summary>
    /// Gets or sets the document-status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets the business records linked to this document.
    /// </summary>
    public ICollection<DocumentLink> DocumentLinks { get; } = new List<DocumentLink>();
}
