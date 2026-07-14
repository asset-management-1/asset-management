namespace Haven.Domain.Entities;

/// <summary>
/// Links one evidence document to one meter record.
/// </summary>
public class DocumentLink : BaseEntity
{
    public long DocumentId { get; set; }
    public long EntityTypeId { get; set; }
    public long EntityId { get; set; }
    public long? LinkTypeId { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public long StatusId { get; set; }
    public Document Document { get; set; }
}
