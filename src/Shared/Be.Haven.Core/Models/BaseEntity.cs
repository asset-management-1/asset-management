namespace Be.Haven.Core.Models;

/// <summary>
/// Base entity for all domain models.
/// </summary>
public abstract class BaseEntity : IIsDeleted
{
    /// <summary>
    /// Internal primary key used for database relations and joins.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Public identifier used for API exposure to avoid predictable IDs.
    /// </summary>
    public Guid PublicId { get; set; }

    /// <summary>
    /// UTC date when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User ID who created the record.
    /// Null for system-generated records.
    /// </summary>
    public long? CreatedBy { get; set; }

    /// <summary>
    /// UTC date when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who last updated the record.
    /// Null if the record has never been updated or was updated by system.
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }
}