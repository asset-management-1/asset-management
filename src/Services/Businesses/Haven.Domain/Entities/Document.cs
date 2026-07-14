namespace Haven.Domain.Entities;

/// <summary>
/// Represents object-storage metadata for a utility evidence file.
/// </summary>
public class Document : BaseEntity
{
    public long DocumentTypeId { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string ContentType { get; set; }
    public string FileExtension { get; set; }
    public long? FileSize { get; set; }
    public long StorageProviderId { get; set; }
    public string StoragePath { get; set; }
    public string FileUrl { get; set; }
    public string Checksum { get; set; }
    public long StatusId { get; set; }
    public ICollection<DocumentLink> DocumentLinks { get; } = new List<DocumentLink>();
}
