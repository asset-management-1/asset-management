using Be.Haven.Core.Interfaces.Softs.IIsActive;

namespace Be.Haven.Core.Models;

/// <summary>
/// Represents the base class for all entities in the domain model.
/// </summary>
public abstract class BaseEntity : IIsActive
{
    /// <summary>
    /// Gets or sets the unique identifier for the plate request.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the user who created the record.
    /// </summary>
    public Guid CreateBy { get; set; }

    /// <summary>
    /// Date and time when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the record.
    /// </summary>
    public Guid? UpdateBy { get; set; }

    /// <summary>
    /// Date and time when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is marked as active or not.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
