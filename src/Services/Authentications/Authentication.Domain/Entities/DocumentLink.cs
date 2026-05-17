namespace Authentication.Domain.Entities;

/// <summary>
/// Represents a generic link between one document and one business entity.
/// </summary>
public class DocumentLink : BaseEntity
{
    /// <summary>
    /// Gets or sets the linked document id.
    /// </summary>
    public long DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the linked entity type master-data value id.
    /// </summary>
    public long EntityTypeId { get; set; }

    /// <summary>
    /// Gets or sets the internal id of the linked entity.
    /// </summary>
    public long EntityId { get; set; }

    /// <summary>
    /// Gets or sets the document-link type master-data value id.
    /// </summary>
    public long? LinkTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the linked document is primary.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets the ordering hint for documents of the same entity.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the link status master-data value id.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets the linked document.
    /// </summary>
    public virtual Document Document { get; set; }

    /// <summary>
    /// Gets or sets the linked entity type master-data value.
    /// </summary>
    public virtual MasterDataValue EntityType { get; set; }

    /// <summary>
    /// Gets or sets the document-link type master-data value.
    /// </summary>
    public virtual MasterDataValue LinkType { get; set; }

    /// <summary>
    /// Gets or sets the link status master-data value.
    /// </summary>
    public virtual MasterDataValue Status { get; set; }
}
