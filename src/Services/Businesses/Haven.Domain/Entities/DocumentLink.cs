namespace Haven.Domain.Entities;

/// <summary>
/// Links one evidence document to one meter record.
/// </summary>
public class DocumentLink : BaseEntity
{
    /// <summary>
    /// Gets or sets the linked document identifier.
    /// </summary>
    public long DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the linked business-entity type identifier.
    /// </summary>
    public long EntityTypeId { get; set; }

    /// <summary>
    /// Gets or sets the linked business-entity identifier.
    /// </summary>
    public long EntityId { get; set; }

    /// <summary>
    /// Gets or sets the optional link-type master-data identifier.
    /// </summary>
    public long? LinkTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary document link.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets the display order within the linked evidence collection.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the link-status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets the linked document navigation.
    /// </summary>
    public Document Document { get; set; }
}
