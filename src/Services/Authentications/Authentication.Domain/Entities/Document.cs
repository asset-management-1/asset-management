namespace Authentication.Domain.Entities;

/// <summary>
/// Represents private file metadata stored for business documents.
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// Gets or sets the document type master-data value id.
    /// </summary>
    public long DocumentTypeId { get; set; }

    /// <summary>
    /// Gets or sets the storage file name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the original file name uploaded by the user.
    /// </summary>
    public string OriginalFileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public string FileExtension { get; set; }

    /// <summary>
    /// Gets or sets the uploaded file size in bytes.
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Gets or sets the storage provider master-data value id.
    /// </summary>
    public long StorageProviderId { get; set; }

    /// <summary>
    /// Gets or sets the private storage object key or path.
    /// </summary>
    public string StoragePath { get; set; }

    /// <summary>
    /// Gets or sets the public file URL when a document is safe to expose.
    /// </summary>
    public string FileUrl { get; set; }

    /// <summary>
    /// Gets or sets the content checksum.
    /// </summary>
    public string Checksum { get; set; }

    /// <summary>
    /// Gets or sets the document status master-data value id.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets the document type master-data value.
    /// </summary>
    public virtual MasterDataValue DocumentType { get; set; }

    /// <summary>
    /// Gets or sets the storage provider master-data value.
    /// </summary>
    public virtual MasterDataValue StorageProvider { get; set; }

    /// <summary>
    /// Gets or sets the document status master-data value.
    /// </summary>
    public virtual MasterDataValue Status { get; set; }

    /// <summary>
    /// Gets or sets links that attach this document to business entities.
    /// </summary>
    public virtual ICollection<DocumentLink> DocumentLinks { get; set; } = new List<DocumentLink>();
}
